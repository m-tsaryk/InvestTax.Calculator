import * as cdk from 'aws-cdk-lib';
import * as s3 from 'aws-cdk-lib/aws-s3';
import * as dynamodb from 'aws-cdk-lib/aws-dynamodb';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as logs from 'aws-cdk-lib/aws-logs';
import * as sfn from 'aws-cdk-lib/aws-stepfunctions';
import * as tasks from 'aws-cdk-lib/aws-stepfunctions-tasks';
import * as s3n from 'aws-cdk-lib/aws-s3-notifications';
import { Construct } from 'constructs';
import * as fs from 'fs';
import * as path from 'path';

export interface InvestTaxStackProps extends cdk.StackProps {
  stage: string;
}

export class InvestTaxStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: InvestTaxStackProps) {
    super(scope, id, props);

    const stage = props.stage;

    // ==================== S3 BUCKETS ====================
    
    // Upload bucket (user uploads)
    const uploadBucket = new s3.Bucket(this, 'UploadBucket', {
      bucketName: `investtax-upload-${stage}`,
      versioned: false,
      encryption: s3.BucketEncryption.S3_MANAGED,
      blockPublicAccess: s3.BlockPublicAccess.BLOCK_ALL,
      lifecycleRules: [
        {
          expiration: cdk.Duration.days(30),
        },
      ],
      removalPolicy: stage === 'prod' ? cdk.RemovalPolicy.RETAIN : cdk.RemovalPolicy.DESTROY,
      autoDeleteObjects: stage !== 'prod',
    });

    // Processing bucket (intermediate files)
    const processingBucket = new s3.Bucket(this, 'ProcessingBucket', {
      bucketName: `investtax-processing-${stage}`,
      versioned: false,
      encryption: s3.BucketEncryption.S3_MANAGED,
      blockPublicAccess: s3.BlockPublicAccess.BLOCK_ALL,
      lifecycleRules: [
        {
          expiration: cdk.Duration.days(7),
        },
      ],
      removalPolicy: stage === 'prod' ? cdk.RemovalPolicy.RETAIN : cdk.RemovalPolicy.DESTROY,
      autoDeleteObjects: stage !== 'prod',
    });

    // ==================== DYNAMODB TABLE ====================
    
    const jobsTable = new dynamodb.Table(this, 'JobsTable', {
      tableName: `InvestTax-Jobs-${stage}`,
      partitionKey: {
        name: 'JobId',
        type: dynamodb.AttributeType.STRING,
      },
      billingMode: dynamodb.BillingMode.PAY_PER_REQUEST,
      removalPolicy: stage === 'prod' ? cdk.RemovalPolicy.RETAIN : cdk.RemovalPolicy.DESTROY,
      pointInTimeRecovery: stage === 'prod',
    });

    // GSI for querying by status
    jobsTable.addGlobalSecondaryIndex({
      indexName: 'StatusIndex',
      partitionKey: {
        name: 'Status',
        type: dynamodb.AttributeType.STRING,
      },
      sortKey: {
        name: 'CreatedAt',
        type: dynamodb.AttributeType.STRING,
      },
    });

    // ==================== IAM ROLES ====================
    
    // Common Lambda execution role
    const lambdaExecutionRole = new iam.Role(this, 'LambdaExecutionRole', {
      assumedBy: new iam.ServicePrincipal('lambda.amazonaws.com'),
      managedPolicies: [
        iam.ManagedPolicy.fromAwsManagedPolicyName('service-role/AWSLambdaBasicExecutionRole'),
        iam.ManagedPolicy.fromAwsManagedPolicyName('AWSXRayDaemonWriteAccess'),
      ],
    });

    // Grant permissions
    uploadBucket.grantReadWrite(lambdaExecutionRole);
    processingBucket.grantReadWrite(lambdaExecutionRole);
    jobsTable.grantReadWriteData(lambdaExecutionRole);

    // SES permissions
    lambdaExecutionRole.addToPolicy(new iam.PolicyStatement({
      actions: ['ses:SendEmail', 'ses:SendRawEmail'],
      resources: ['*'],
    }));

    // ==================== LAMBDA FUNCTIONS ====================
    
    // Validator Lambda
    const validatorFunction = new lambda.Function(this, 'ValidatorFunction', {
      functionName: `InvestTax-Validator-${stage}`,
      runtime: lambda.Runtime.DOTNET_8,
      handler: 'InvestTax.Lambda.Validator::InvestTax.Lambda.Validator.Function::FunctionHandler',
      code: lambda.Code.fromAsset('../../src/InvestTax.Lambda.Validator/bin/Release/net10.0/publish'),
      role: lambdaExecutionRole,
      timeout: cdk.Duration.minutes(5),
      memorySize: 512,
      environment: {
        UPLOAD_BUCKET: uploadBucket.bucketName,
        PROCESSING_BUCKET: processingBucket.bucketName,
        JOBS_TABLE: jobsTable.tableName,
        STAGE: stage,
      },
      logRetention: logs.RetentionDays.ONE_WEEK,
    });

    // Normalizer Lambda
    const normalizerFunction = new lambda.Function(this, 'NormalizerFunction', {
      functionName: `InvestTax-Normalizer-${stage}`,
      runtime: lambda.Runtime.DOTNET_8,
      handler: 'InvestTax.Lambda.Normalizer::InvestTax.Lambda.Normalizer.Function::FunctionHandler',
      code: lambda.Code.fromAsset('../../src/InvestTax.Lambda.Normalizer/bin/Release/net10.0/publish'),
      role: lambdaExecutionRole,
      timeout: cdk.Duration.minutes(5),
      memorySize: 512,
      environment: {
        PROCESSING_BUCKET: processingBucket.bucketName,
        JOBS_TABLE: jobsTable.tableName,
        STAGE: stage,
      },
      logRetention: logs.RetentionDays.ONE_WEEK,
    });

    // NBP Client Lambda
    const nbpClientFunction = new lambda.Function(this, 'NBPClientFunction', {
      functionName: `InvestTax-NBPClient-${stage}`,
      runtime: lambda.Runtime.DOTNET_8,
      handler: 'InvestTax.Lambda.NBPClient::InvestTax.Lambda.NBPClient.Function::FunctionHandler',
      code: lambda.Code.fromAsset('../../src/InvestTax.Lambda.NBPClient/bin/Release/net10.0/publish'),
      role: lambdaExecutionRole,
      timeout: cdk.Duration.minutes(5),
      memorySize: 512,
      environment: {
        PROCESSING_BUCKET: processingBucket.bucketName,
        JOBS_TABLE: jobsTable.tableName,
        NBP_API_URL: 'https://api.nbp.pl/api/exchangerates',
        STAGE: stage,
      },
      logRetention: logs.RetentionDays.ONE_WEEK,
    });

    // Calculator Lambda
    const calculatorFunction = new lambda.Function(this, 'CalculatorFunction', {
      functionName: `InvestTax-Calculator-${stage}`,
      runtime: lambda.Runtime.DOTNET_8,
      handler: 'InvestTax.Lambda.Calculator::InvestTax.Lambda.Calculator.Function::FunctionHandler',
      code: lambda.Code.fromAsset('../../src/InvestTax.Lambda.Calculator/bin/Release/net10.0/publish'),
      role: lambdaExecutionRole,
      timeout: cdk.Duration.minutes(10),
      memorySize: 1024,
      environment: {
        PROCESSING_BUCKET: processingBucket.bucketName,
        JOBS_TABLE: jobsTable.tableName,
        STAGE: stage,
      },
      logRetention: logs.RetentionDays.ONE_WEEK,
    });

    // Report Generator Lambda
    const reportGeneratorFunction = new lambda.Function(this, 'ReportGeneratorFunction', {
      functionName: `InvestTax-ReportGenerator-${stage}`,
      runtime: lambda.Runtime.DOTNET_8,
      handler: 'InvestTax.Lambda.ReportGenerator::InvestTax.Lambda.ReportGenerator.Function::FunctionHandler',
      code: lambda.Code.fromAsset('../../src/InvestTax.Lambda.ReportGenerator/bin/Release/net10.0/publish'),
      role: lambdaExecutionRole,
      timeout: cdk.Duration.minutes(5),
      memorySize: 512,
      environment: {
        PROCESSING_BUCKET: processingBucket.bucketName,
        JOBS_TABLE: jobsTable.tableName,
        STAGE: stage,
      },
      logRetention: logs.RetentionDays.ONE_WEEK,
    });

    // Email Sender Lambda
    const emailSenderFunction = new lambda.Function(this, 'EmailSenderFunction', {
      functionName: `InvestTax-EmailSender-${stage}`,
      runtime: lambda.Runtime.DOTNET_8,
      handler: 'InvestTax.Lambda.EmailSender::InvestTax.Lambda.EmailSender.Function::FunctionHandler',
      code: lambda.Code.fromAsset('../../src/InvestTax.Lambda.EmailSender/bin/Release/net10.0/publish'),
      role: lambdaExecutionRole,
      timeout: cdk.Duration.minutes(3),
      memorySize: 256,
      environment: {
        PROCESSING_BUCKET: processingBucket.bucketName,
        JOBS_TABLE: jobsTable.tableName,
        SES_FROM_EMAIL: `noreply@investtax-${stage}.example.com`,
        STAGE: stage,
      },
      logRetention: logs.RetentionDays.ONE_WEEK,
    });

    // ==================== STEP FUNCTIONS STATE MACHINE ====================
    
    // Load state machine definition from JSON file
    const stateMachineDefinitionPath = path.join(__dirname, 'state-machine-definition.json');
    let stateMachineDefinitionString = fs.readFileSync(stateMachineDefinitionPath, 'utf8');

    // Replace placeholders with actual resource names and ARNs
    stateMachineDefinitionString = stateMachineDefinitionString
      .replace(/\$\{JOBS_TABLE\}/g, jobsTable.tableName)
      .replace(/\$\{VALIDATOR_FUNCTION\}/g, validatorFunction.functionArn)
      .replace(/\$\{NORMALIZER_FUNCTION\}/g, normalizerFunction.functionArn)
      .replace(/\$\{NBP_CLIENT_FUNCTION\}/g, nbpClientFunction.functionArn)
      .replace(/\$\{CALCULATOR_FUNCTION\}/g, calculatorFunction.functionArn)
      .replace(/\$\{REPORT_GENERATOR_FUNCTION\}/g, reportGeneratorFunction.functionArn)
      .replace(/\$\{EMAIL_SENDER_FUNCTION\}/g, emailSenderFunction.functionArn);

    const stateMachineDefinition = JSON.parse(stateMachineDefinitionString);

    // Create state machine with definition from JSON
    const stateMachine = new sfn.StateMachine(this, 'TaxCalculationWorkflow', {
      stateMachineName: `InvestTax-Workflow-${stage}`,
      definitionBody: sfn.DefinitionBody.fromString(JSON.stringify(stateMachineDefinition)),
      timeout: cdk.Duration.minutes(30),
      tracingEnabled: true,
      logs: {
        destination: new logs.LogGroup(this, 'StateMachineLogGroup', {
          logGroupName: `/aws/stepfunctions/InvestTax-Workflow-${stage}`,
          retention: logs.RetentionDays.ONE_WEEK,
          removalPolicy: cdk.RemovalPolicy.DESTROY,
        }),
        level: sfn.LogLevel.ALL,
        includeExecutionData: true,
      },
    });

    // Grant Step Functions permission to invoke Lambdas
    validatorFunction.grantInvoke(stateMachine);
    normalizerFunction.grantInvoke(stateMachine);
    nbpClientFunction.grantInvoke(stateMachine);
    calculatorFunction.grantInvoke(stateMachine);
    reportGeneratorFunction.grantInvoke(stateMachine);
    emailSenderFunction.grantInvoke(stateMachine);

    // Grant Step Functions permission to update DynamoDB
    jobsTable.grantReadWriteData(stateMachine);

    // ==================== S3 EVENT TRIGGER ====================
    
    // Create Lambda to trigger Step Functions on S3 upload
    const triggerFunction = new lambda.Function(this, 'StarterFunction', {
      functionName: `InvestTax-Starter-${stage}`,
      runtime: lambda.Runtime.DOTNET_8,
      handler: 'InvestTax.Lambda.Starter::InvestTax.Lambda.Starter.Function::FunctionHandler',
      code: lambda.Code.fromAsset('../../src/InvestTax.Lambda.Starter/bin/Release/net10.0/publish'),
      role: lambdaExecutionRole,
      timeout: cdk.Duration.seconds(30),
      memorySize: 256,
      environment: {
        STATE_MACHINE_ARN: stateMachine.stateMachineArn,
        JOBS_TABLE: jobsTable.tableName,
        PROCESSING_BUCKET: processingBucket.bucketName,
        STAGE: stage,
      },
      logRetention: logs.RetentionDays.ONE_WEEK,
    });

    // Grant permissions to trigger function
    stateMachine.grantStartExecution(triggerFunction);
    jobsTable.grantWriteData(triggerFunction);

    // Add S3 event notification
    uploadBucket.addEventNotification(
      s3.EventType.OBJECT_CREATED,
      new s3n.LambdaDestination(triggerFunction),
      {
        suffix: '.csv',
      }
    );

    // ==================== OUTPUTS ====================
    
    new cdk.CfnOutput(this, 'UploadBucketName', {
      value: uploadBucket.bucketName,
      description: 'S3 bucket for user uploads',
      exportName: `InvestTax-UploadBucket-${stage}`,
    });

    new cdk.CfnOutput(this, 'ProcessingBucketName', {
      value: processingBucket.bucketName,
      description: 'S3 bucket for processing',
      exportName: `InvestTax-ProcessingBucket-${stage}`,
    });

    new cdk.CfnOutput(this, 'JobsTableName', {
      value: jobsTable.tableName,
      description: 'DynamoDB table for job tracking',
      exportName: `InvestTax-JobsTable-${stage}`,
    });

    new cdk.CfnOutput(this, 'StateMachineArn', {
      value: stateMachine.stateMachineArn,
      description: 'Step Functions state machine ARN',
      exportName: `InvestTax-StateMachine-${stage}`,
    });
  }
}
