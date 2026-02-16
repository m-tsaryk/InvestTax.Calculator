# InvestTax Calculator - Architecture Documentation

This folder contains the complete architecture documentation for the InvestTax Calculator system, organized into logical sections for easy navigation and reference.

## Quick Navigation

### 📋 Getting Started
- **[Overview](overview.md)** - Executive summary and high-level architecture approach
- **[System Context](system-context.md)** - External actors, boundaries, and interactions

### 🏗️ Architecture Design
- **[Component Architecture](component-architecture.md)** - Detailed component structure and layers
- **[Deployment Architecture](deployment-architecture.md)** - AWS infrastructure and deployment strategy
- **[Data Flow](data-flow.md)** - End-to-end data processing pipeline
- **[Workflows](workflows.md)** - Sequence diagrams for key scenarios

### 📊 Planning & Analysis
- **[Phased Development](phased-development.md)** - MVP and production-ready phases
- **[NFR Analysis](nfr-analysis.md)** - Non-functional requirements (scalability, performance, security, reliability, maintainability)
- **[Risks & Technology Stack](risks-and-tech-stack.md)** - Risk mitigation and technology choices

### 🚀 Implementation
- **[Implementation Guide](implementation-guide.md)** - Next steps, timelines, and success criteria

---

## Document Purpose

Each document is designed to be standalone yet interconnected, allowing you to:
- **Developers**: Understand component responsibilities and implementation details
- **Architects**: Review design decisions and architectural patterns
- **DevOps**: Plan infrastructure deployment and operational procedures
- **Project Managers**: Track phases, timelines, and resource requirements
- **Security Teams**: Review security controls and compliance considerations

## Architecture at a Glance

**Type**: Serverless, Event-Driven Microservices  
**Cloud Provider**: AWS (Region: eu-central-1)  
**Key Services**: Lambda, Step Functions, S3, DynamoDB, SES  
**Processing Time**: < 5 minutes for 100K rows  
**Availability Target**: 99.9%  
**Cost Estimate**: $100-500/month

## Key Design Principles

1. **Serverless-First**: Zero server management, automatic scaling
2. **Event-Driven**: Asynchronous processing via S3 events and Step Functions
3. **Security-Focused**: Encryption at rest/transit, least-privilege IAM
4. **Cost-Optimized**: Pay-per-use with no idle costs
5. **Observable**: Comprehensive logging, metrics, and distributed tracing

## How to Use This Documentation

### For First-Time Readers
1. Start with [Overview](overview.md) for context
2. Review [System Context](system-context.md) to understand boundaries
3. Deep-dive into [Component Architecture](component-architecture.md)

### For Implementation
1. Review [Phased Development](phased-development.md) for timeline
2. Study [Component Architecture](component-architecture.md) and [Data Flow](data-flow.md)
3. Follow [Implementation Guide](implementation-guide.md) for step-by-step tasks

### For Operations
1. Understand [Deployment Architecture](deployment-architecture.md)
2. Review [NFR Analysis](nfr-analysis.md) for operational requirements
3. Study [Workflows](workflows.md) for error handling scenarios

---

## Document Maintenance

**Last Updated**: February 16, 2026  
**Version**: 1.0  
**Author**: Senior Cloud Architect  
**Review Cycle**: Monthly or before major releases

## Related Documentation

- [PRD](../prd.md) - Product Requirements Document
- [User Stories](../user-stories.md) - Detailed user requirements

---

**Questions or Feedback?** Contact the architecture team or create an issue in the project repository.
