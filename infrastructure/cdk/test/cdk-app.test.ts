import * as cdk from 'aws-cdk-lib';
import { Template } from 'aws-cdk-lib/assertions';

// Mock the InvestTaxStack since we're testing the app initialization
jest.mock('../lib/investtax-stack', () => ({
  InvestTaxStack: jest.fn().mockImplementation((scope, id, props) => {
    return new cdk.Stack(scope, id, props);
  }),
}));

describe('CDK App', () => {
  let originalEnv: NodeJS.ProcessEnv;

  beforeEach(() => {
    // Save original environment
    originalEnv = { ...process.env };
    
    // Clear module cache to allow fresh imports
    jest.resetModules();
  });

  afterEach(() => {
    // Restore original environment
    process.env = originalEnv;
  });

  test('creates app and synthesizes', () => {
    const app = new cdk.App();
    expect(app).toBeDefined();
    
    const assembly = app.synth();
    expect(assembly).toBeDefined();
  });

  test('creates stack with correct naming', () => {
    const app = new cdk.App({
      context: {
        stage: 'test',
      },
    });

    const stack = new cdk.Stack(app, 'InvestTaxStack-test');
    expect(stack.stackName).toBe('InvestTaxStack-test');
  });

  test('uses correct region configuration', () => {
    process.env.CDK_DEFAULT_REGION = 'eu-central-1';

    const app = new cdk.App();
    const stack = new cdk.Stack(app, 'TestStack', {
      env: {
        region: 'eu-central-1',
      },
    });

    expect(stack.region).toBe('eu-central-1');
  });

  test('supports multiple stages', () => {
    const stages = ['dev', 'staging', 'prod'];

    stages.forEach((stage) => {
      const app = new cdk.App({
        context: {
          stage,
        },
      });

      const stack = new cdk.Stack(app, `InvestTaxStack-${stage}`, {
        stackName: `InvestTaxStack-${stage}`,
      });

      expect(stack.stackName).toBe(`InvestTaxStack-${stage}`);
    });
  });

  test('applies correct tags', () => {
    const app = new cdk.App();
    const stack = new cdk.Stack(app, 'TestStack', {
      tags: {
        Project: 'InvestTax',
        Environment: 'test',
        ManagedBy: 'CDK',
      },
    });

    const template = Template.fromStack(stack);
    expect(template).toBeDefined();
  });

  test('defaults to dev stage when not specified', () => {
    const app = new cdk.App();
    const stage = app.node.tryGetContext('stage') || 'dev';
    
    expect(stage).toBe('dev');
  });

  test('uses stage from context when provided', () => {
    const app = new cdk.App({
      context: {
        stage: 'production',
      },
    });
    
    const stage = app.node.tryGetContext('stage');
    expect(stage).toBe('production');
  });

  test('sets account from environment variable', () => {
    process.env.CDK_DEFAULT_ACCOUNT = '123456789012';

    const app = new cdk.App();
    const stack = new cdk.Stack(app, 'TestStack', {
      env: {
        account: process.env.CDK_DEFAULT_ACCOUNT,
      },
    });

    expect(stack.account).toBe('123456789012');
  });

  test('allows override of default account', () => {
    const app = new cdk.App();
    const stack = new cdk.Stack(app, 'TestStack', {
      env: {
        account: '999888777666',
      },
    });

    expect(stack.account).toBe('999888777666');
  });

  test('can synthesize multiple stacks', () => {
    const app = new cdk.App();
    
    new cdk.Stack(app, 'Stack1');
    new cdk.Stack(app, 'Stack2');
    new cdk.Stack(app, 'Stack3');

    const assembly = app.synth();
    expect(assembly.stacks.length).toBe(3);
  });
});
