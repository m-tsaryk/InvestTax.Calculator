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
    
    // Define Step Functions tasks
    const validateTask = new tasks.LambdaInvoke(this, 'ValidateCSV', {
      lambdaFunction: validatorFunction,
      outputPath: '$.Payload',
    });

    const normalizeTask = new tasks.LambdaInvoke(this, 'NormalizeData', {
      lambdaFunction: normalizerFunction,
      outputPath: '$.Payload',
    });

    const fetchRatesTask = new tasks.LambdaInvoke(this, 'FetchNBPRates', {
      lambdaFunction: nbpClientFunction,
      outputPath: '$.Payload',
    });

    const calculateTask = new tasks.LambdaInvoke(this, 'CalculateTax', {
      lambdaFunction: calculatorFunction,
      outputPath: '$.Payload',
    });

    const generateReportTask = new tasks.LambdaInvoke(this, 'GenerateReport', {
      lambdaFunction: reportGeneratorFunction,
      outputPath: '$.Payload',
    });

    const sendEmailTask = new tasks.LambdaInvoke(this, 'SendEmail', {
      lambdaFunction: emailSenderFunction,
      outputPath: '$.Payload',
    });

    // Error handling states
    const jobFailed = new sfn.Fail(this, 'JobFailed', {
      cause: 'Job processing failed',
      error: 'ProcessingError',
    });

    const jobSucceeded = new sfn.Succeed(this, 'JobSucceeded', {
      comment: 'Job completed successfully',
    });

    // Define workflow
    const definition = validateTask
      .addCatch(jobFailed, {
        resultPath: '$.error',
      })
      .next(normalizeTask
        .addCatch(jobFailed, {
          resultPath: '$.error',
        }))
      .next(fetchRatesTask
        .addCatch(jobFailed, {
          resultPath: '$.error',
        }))
      .next(calculateTask
        .addCatch(jobFailed, {
          resultPath: '$.error',
        }))
      .next(generateReportTask
        .addCatch(jobFailed, {
          resultPath: '$.error',
        }))
      .next(sendEmailTask
        .addCatch(jobFailed, {
          resultPath: '$.error',
        }))
      .next(jobSucceeded);

    // Create state machine
    const stateMachine = new sfn.StateMachine(this, 'TaxCalculationWorkflow', {
      stateMachineName: `InvestTax-Workflow-${stage}`,
      definition: definition,
      timeout: cdk.Duration.minutes(30),
      logs: {
        destination: new logs.LogGroup(this, 'StateMachineLogGroup', {
          logGroupName: `/aws/stepfunctions/InvestTax-Workflow-${stage}`,
          retention: logs.RetentionDays.ONE_WEEK,
          removalPolicy: cdk.RemovalPolicy.DESTROY,
        }),
        level: sfn.LogLevel.ALL,
      },
    });

    // Grant Step Functions permission to invoke Lambdas
    validatorFunction.grantInvoke(stateMachine);
    normalizerFunction.grantInvoke(stateMachine);
    nbpClientFunction.grantInvoke(stateMachine);
    calculatorFunction.grantInvoke(stateMachine);
    reportGeneratorFunction.grantInvoke(stateMachine);
    emailSenderFunction.grantInvoke(stateMachine);

    // ==================== S3 EVENT TRIGGER ====================
    
    // Create Lambda to trigger Step Functions on S3 upload
    const triggerFunction = new lambda.Function(this, 'TriggerFunction', {
      functionName: `InvestTax-Trigger-${stage}`,
      runtime: lambda.Runtime.PYTHON_3_12,
      handler: 'index.handler',
      code: lambda.Code.fromInline(`
import json
import boto3
import os

stepfunctions = boto3.client('stepfunctions')
dynamodb = boto3.resource('dynamodb')

def handler(event, context):
    state_machine_arn = os.environ['STATE_MACHINE_ARN']
    jobs_table_name = os.environ['JOBS_TABLE']
    
    for record in event['Records']:
        bucket = record['s3']['bucket']['name']
        key = record['s3']['object']['key']
        
        # Extract email from S3 object metadata or key
        # For MVP, assume email is in the key: email@example.com/file.csv
        parts = key.split('/')
        if len(parts) != 2:
            print(f'Invalid S3 key format: {key}')
            continue
        
        email = parts[0]
        filename = parts[1]
        
        # Create job in DynamoDB
        import uuid
        job_id = str(uuid.uuid4())
        
        table = dynamodb.Table(jobs_table_name)
        table.put_item(Item={
            'JobId': job_id,
            'Email': email,
            'S3Key': key,
            'Status': 'Created',
            'CreatedAt': context.aws_request_id,
        })
        
        # Start Step Functions execution
        execution_input = {
            'JobId': job_id,
            'Email': email,
            'S3Key': key,
            'UploadBucket': bucket,
            'ProcessingBucket': os.environ['PROCESSING_BUCKET'],
        }
        
        response = stepfunctions.start_execution(
            stateMachineArn=state_machine_arn,
            name=job_id,
            input=json.dumps(execution_input)
        )
        
        print(f'Started execution for job {job_id}: {response["executionArn"]}')
    
    return {
        'statusCode': 200,
        'body': json.dumps('Processing started')
    }
      `),
      environment: {
        STATE_MACHINE_ARN: stateMachine.stateMachineArn,
        JOBS_TABLE: jobsTable.tableName,
        PROCESSING_BUCKET: processingBucket.bucketName,
      },
      timeout: cdk.Duration.seconds(30),
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
