import * as fs from 'fs';
import * as path from 'path';

describe('State Machine Definition', () => {
  let stateMachineDefinition: any;

  beforeAll(() => {
    const definitionPath = path.join(__dirname, '../lib/state-machine-definition.json');
    const definitionString = fs.readFileSync(definitionPath, 'utf8');
    stateMachineDefinition = JSON.parse(definitionString);
  });

  test('has valid JSON structure', () => {
    expect(stateMachineDefinition).toBeDefined();
    expect(typeof stateMachineDefinition).toBe('object');
  });

  test('has required top-level properties', () => {
    expect(stateMachineDefinition.Comment).toBeDefined();
    expect(stateMachineDefinition.StartAt).toBeDefined();
    expect(stateMachineDefinition.States).toBeDefined();
  });

  test('starts with correct initial state', () => {
    expect(stateMachineDefinition.StartAt).toBe('ExtractMetadata');
    expect(stateMachineDefinition.States.ExtractMetadata).toBeDefined();
  });

  test('contains all required states', () => {
    const requiredStates = [
      'ExtractMetadata',
      'UpdateJobProcessing',
      'ValidateCSV',
      'CheckValidationResult',
      'UpdateJobNormalizing',
      'NormalizeData',
      'UpdateJobFetchingRates',
      'FetchNBPRates',
      'CheckRatesFetched',
      'UpdateJobCalculating',
      'CalculateTax',
      'UpdateJobGeneratingReport',
      'GenerateReport',
      'UpdateJobSendingEmail',
      'SendEmail',
      'UpdateJobSuccess',
      'JobSucceeded',
      'SendValidationErrorEmail',
      'SendRateErrorEmail',
      'SendProcessingErrorEmail',
      'UpdateJobFailedValidation',
      'UpdateJobFailedRate',
      'UpdateJobFailed',
      'JobFailed',
    ];

    requiredStates.forEach((state) => {
      expect(stateMachineDefinition.States[state]).toBeDefined();
    });
  });

  test('all Lambda invocation states use correct task type', () => {
    const lambdaStates = [
      'ValidateCSV',
      'NormalizeData',
      'FetchNBPRates',
      'CalculateTax',
      'GenerateReport',
      'SendEmail',
      'SendValidationErrorEmail',
      'SendRateErrorEmail',
      'SendProcessingErrorEmail',
    ];

    lambdaStates.forEach((state) => {
      const stateDefinition = stateMachineDefinition.States[state];
      expect(stateDefinition.Type).toBe('Task');
      expect(stateDefinition.Resource).toContain('lambda:invoke');
    });
  });

  test('all Lambda states have retry configuration', () => {
    const lambdaStates = [
      'ValidateCSV',
      'NormalizeData',
      'FetchNBPRates',
      'CalculateTax',
      'GenerateReport',
      'SendEmail',
    ];

    lambdaStates.forEach((state) => {
      const stateDefinition = stateMachineDefinition.States[state];
      expect(stateDefinition.Retry).toBeDefined();
      expect(Array.isArray(stateDefinition.Retry)).toBe(true);
      expect(stateDefinition.Retry.length).toBeGreaterThan(0);
    });
  });

  test('all states have valid next or end configuration', () => {
    Object.entries(stateMachineDefinition.States).forEach(([stateName, state]: [string, any]) => {
      if (state.Type !== 'Succeed' && state.Type !== 'Fail') {
        const hasNext = state.Next !== undefined;
        const hasEnd = state.End === true;
        const hasChoices = state.Type === 'Choice';

        expect(hasNext || hasEnd || hasChoices).toBe(true);
      }
    });
  });

  test('DynamoDB update states use correct resource ARN', () => {
    const dynamoStates = [
      'UpdateJobProcessing',
      'UpdateJobNormalizing',
      'UpdateJobFetchingRates',
      'UpdateJobCalculating',
      'UpdateJobGeneratingReport',
      'UpdateJobSendingEmail',
      'UpdateJobSuccess',
      'UpdateJobFailedValidation',
      'UpdateJobFailedRate',
      'UpdateJobFailed',
    ];

    dynamoStates.forEach((state) => {
      const stateDefinition = stateMachineDefinition.States[state];
      expect(stateDefinition.Resource).toContain('dynamodb:updateItem');
    });
  });

  test('contains placeholder variables for resource substitution', () => {
    const definitionString = JSON.stringify(stateMachineDefinition);

    expect(definitionString).toContain('${JOBS_TABLE}');
    expect(definitionString).toContain('${VALIDATOR_FUNCTION}');
    expect(definitionString).toContain('${NORMALIZER_FUNCTION}');
    expect(definitionString).toContain('${NBP_CLIENT_FUNCTION}');
    expect(definitionString).toContain('${CALCULATOR_FUNCTION}');
    expect(definitionString).toContain('${REPORT_GENERATOR_FUNCTION}');
    expect(definitionString).toContain('${EMAIL_SENDER_FUNCTION}');
  });

  test('CheckValidationResult is a Choice state', () => {
    const state = stateMachineDefinition.States.CheckValidationResult;
    expect(state.Type).toBe('Choice');
    expect(state.Choices).toBeDefined();
    expect(Array.isArray(state.Choices)).toBe(true);
  });

  test('CheckRatesFetched is a Choice state', () => {
    const state = stateMachineDefinition.States.CheckRatesFetched;
    expect(state.Type).toBe('Choice');
    expect(state.Choices).toBeDefined();
    expect(Array.isArray(state.Choices)).toBe(true);
  });

  test('has error handling with Catch blocks', () => {
    const statesWithCatchBlocks = [
      'UpdateJobProcessing',
      'ValidateCSV',
      'UpdateJobNormalizing',
      'NormalizeData',
      'UpdateJobFetchingRates',
      'FetchNBPRates',
      'UpdateJobCalculating',
      'CalculateTax',
      'UpdateJobGeneratingReport',
      'GenerateReport',
      'UpdateJobSendingEmail',
      'SendEmail',
      'SendValidationErrorEmail',
      'SendRateErrorEmail',
      'SendProcessingErrorEmail',
      'UpdateJobFailed',
    ];

    statesWithCatchBlocks.forEach((stateName) => {
      const state = stateMachineDefinition.States[stateName];
      expect(state.Catch).toBeDefined();
      expect(Array.isArray(state.Catch)).toBe(true);
      expect(state.Catch.length).toBeGreaterThan(0);

      state.Catch.forEach((catchBlock: any) => {
        expect(catchBlock.ErrorEquals).toBeDefined();
        expect(catchBlock.Next).toBeDefined();
      });
    });
  });

  test('all error paths lead to appropriate error states', () => {
    Object.values(stateMachineDefinition.States).forEach((state: any) => {
      if (state.Catch) {
        state.Catch.forEach((catchBlock: any) => {
          const nextState = catchBlock.Next;
          expect(stateMachineDefinition.States[nextState]).toBeDefined();
        });
      }
    });
  });

  test('success path terminates correctly', () => {
    const state = stateMachineDefinition.States.UpdateJobSuccess;
    expect(state.Next).toBe('JobSucceeded');
    
    const succeedState = stateMachineDefinition.States.JobSucceeded;
    expect(succeedState.Type).toBe('Succeed');
  });

  test('failure paths terminate correctly', () => {
    const failState = stateMachineDefinition.States.JobFailed;
    expect(failState.Type).toBe('Fail');
    expect(failState.Error).toBeDefined();
    expect(failState.Cause).toBeDefined();
  });

  test('Lambda states pass correct payload structure', () => {
    const validatorState = stateMachineDefinition.States.ValidateCSV;
    expect(validatorState.Parameters).toBeDefined();
    expect(validatorState.Parameters.FunctionName).toBeDefined();
    expect(validatorState.Parameters.Payload).toBeDefined();
  });

  test('retry configuration has appropriate backoff', () => {
    const validatorState = stateMachineDefinition.States.ValidateCSV;
    const retryConfig = validatorState.Retry[0];

    expect(retryConfig.IntervalSeconds).toBeGreaterThan(0);
    expect(retryConfig.MaxAttempts).toBeGreaterThan(0);
    expect(retryConfig.BackoffRate).toBeGreaterThan(1);
  });

  test('no circular dependencies in state transitions', () => {
    const visited = new Set<string>();
    const recursionStack = new Set<string>();

    const detectCycle = (stateName: string): boolean => {
      if (recursionStack.has(stateName)) {
        return true;
      }

      if (visited.has(stateName)) {
        return false;
      }

      visited.add(stateName);
      recursionStack.add(stateName);

      const state = stateMachineDefinition.States[stateName];
      
      if (state.Next) {
        if (detectCycle(state.Next)) {
          return true;
        }
      }

      if (state.Choices) {
        for (const choice of state.Choices) {
          if (choice.Next && detectCycle(choice.Next)) {
            return true;
          }
        }
      }

      if (state.Catch) {
        for (const catchBlock of state.Catch) {
          if (detectCycle(catchBlock.Next)) {
            return true;
          }
        }
      }

      recursionStack.delete(stateName);
      return false;
    };

    const hasCycle = detectCycle(stateMachineDefinition.StartAt);
    expect(hasCycle).toBe(false);
  });

  test('all referenced states exist', () => {
    const referencedStates = new Set<string>();

    Object.values(stateMachineDefinition.States).forEach((state: any) => {
      if (state.Next) {
        referencedStates.add(state.Next);
      }

      if (state.Choices) {
        state.Choices.forEach((choice: any) => {
          if (choice.Next) {
            referencedStates.add(choice.Next);
          }
        });
      }

      if (state.Default) {
        referencedStates.add(state.Default);
      }

      if (state.Catch) {
        state.Catch.forEach((catchBlock: any) => {
          referencedStates.add(catchBlock.Next);
        });
      }
    });

    referencedStates.forEach((stateName) => {
      expect(stateMachineDefinition.States[stateName]).toBeDefined();
    });
  });

  test('ResultSelector is used appropriately', () => {
    const statesWithResultSelector = [
      'ValidateCSV',
      'NormalizeData',
      'FetchNBPRates',
      'CalculateTax',
      'GenerateReport',
    ];

    statesWithResultSelector.forEach((stateName) => {
      const state = stateMachineDefinition.States[stateName];
      
      expect(state.ResultSelector).toBeDefined();
      expect(typeof state.ResultSelector).toBe('object');
    });
  });

  test('DynamoDB update expressions are properly formatted', () => {
    const dynamoState = stateMachineDefinition.States.UpdateJobProcessing;
    
    expect(dynamoState.Parameters.UpdateExpression).toBeDefined();
    expect(dynamoState.Parameters.ExpressionAttributeNames).toBeDefined();
    expect(dynamoState.Parameters.ExpressionAttributeValues).toBeDefined();
  });

  test('all email sender invocations have required fields', () => {
    const emailStates = [
      'SendEmail',
      'SendValidationErrorEmail',
      'SendRateErrorEmail',
      'SendProcessingErrorEmail',
    ];

    emailStates.forEach((stateName) => {
      const state = stateMachineDefinition.States[stateName];
      expect(state.Parameters.Payload).toBeDefined();
      // Check for JobId field (could be JobId.$ for JSON path)
      const hasJobId = state.Parameters.Payload.JobId || state.Parameters.Payload['JobId.$'];
      expect(hasJobId).toBeDefined();
      // Check for Email field
      const hasEmail = state.Parameters.Payload.Email || state.Parameters.Payload['Email.$'];
      expect(hasEmail).toBeDefined();
      // Check for IsSuccess field
      expect(state.Parameters.Payload.IsSuccess).toBeDefined();
    });
  });
});
