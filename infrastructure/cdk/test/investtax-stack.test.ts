import * as cdk from 'aws-cdk-lib';
import { InvestTaxStack } from '../lib/investtax-stack';
import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';

// Create a temporary directory with a dummy file for Lambda asset
const tempDir = fs.mkdtempSync(path.join(os.tmpdir(), 'lambda-test-'));
fs.writeFileSync(path.join(tempDir, 'dummy.dll'), 'dummy content');

// Mock Lambda Code.fromAsset to use the temp directory
jest.mock('aws-cdk-lib/aws-lambda', () => {
  const actual = jest.requireActual('aws-cdk-lib/aws-lambda');
  return {
    ...actual,
    Code: {
      ...actual.Code,
      fromAsset: jest.fn(() => actual.Code.fromAsset(tempDir)),
    },
  };
});

describe('InvestTaxStack', () => {
  afterAll(() => {
    // Cleanup temp directory
    if (fs.existsSync(tempDir)) {
      fs.rmSync(tempDir, { recursive: true, force: true });
    }
  });

  describe('Stack Creation', () => {
    test('creates stack with dev stage', () => {
      const app = new cdk.App();
      const stack = new InvestTaxStack(app, 'TestStack', {
        stage: 'dev',
      });

      expect(stack).toBeDefined();
      expect(stack.stackName).toBe('TestStack');
    });

    test('creates stack with prod stage', () => {
      const app = new cdk.App();
      const stack = new InvestTaxStack(app, 'TestStack', {
        stage: 'prod',
      });

      expect(stack).toBeDefined();
      expect(stack.stackName).toBe('TestStack');
    });

    test('creates stack with test stage', () => {
      const app = new cdk.App();
      const stack = new InvestTaxStack(app, 'TestStack', {
        stage: 'test',
      });

      expect(stack).toBeDefined();
      expect(stack.stackName).toBe('TestStack');
    });

    test('stack can be synthesized', () => {
      const app = new cdk.App();
      new InvestTaxStack(app, 'TestStack', {
        stage: 'dev',
      });

      const assembly = app.synth();
      expect(assembly).toBeDefined();
      expect(assembly.stacks.length).toBe(1);
    });
  });

  describe('Stage Configuration', () => {
    test('accepts different stage values', () => {
      const stages = ['dev', 'staging', 'prod', 'test'];

      stages.forEach((stage) => {
        const app = new cdk.App();
        const stack = new InvestTaxStack(app, `TestStack-${stage}`, {
          stage,
        });

        expect(stack).toBeDefined();
      });
    });

    test('stage is passed to stack props', () => {
      const app = new cdk.App();
      const stack = new InvestTaxStack(app, 'TestStack', {
        stage: 'production',
      });

      expect(stack).toBeDefined();
    });
  });

  describe('Resource Creation', () => {
    test('stack creates resources without errors', () => {
      const app = new cdk.App();
      
      expect(() => {
        new InvestTaxStack(app, 'TestStack', {
          stage: 'dev',
        });
      }).not.toThrow();
    });

    test('multiple stacks can be created', () => {
      const app = new cdk.App();
      
      const stack1 = new InvestTaxStack(app, 'TestStack1', {
        stage: 'dev',
      });
      
      const stack2 = new InvestTaxStack(app, 'TestStack2', {
        stage: 'staging',
      });

      expect(stack1).toBeDefined();
      expect(stack2).toBeDefined();
      expect(stack1.stackName).not.toBe(stack2.stackName);
    });
  });

  describe('Stack Properties', () => {
    test('stack has required CDK properties', () => {
      const app = new cdk.App();
      const stack = new InvestTaxStack(app, 'TestStack', {
        stage: 'dev',
      });

      expect(stack.stackId).toBeDefined();
      expect(stack.stackName).toBeDefined();
      expect(stack.node).toBeDefined();
    });

    test('stack can be tagged', () => {
      const app = new cdk.App();
      const stack = new InvestTaxStack(app, 'TestStack', {
        stage: 'dev',
        tags: {
          Project: 'InvestTax',
          Environment: 'dev',
        },
      });

      expect(stack).toBeDefined();
    });

    test('stack accepts custom properties', () => {
      const app = new cdk.App();
      const stack = new InvestTaxStack(app, 'TestStack', {
        stage: 'dev',
        description: 'Test stack description',
      });

      expect(stack).toBeDefined();
    });
  });
});
