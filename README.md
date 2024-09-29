<div align="center">

  ![Paralax Logo](./docs/logo/Paralax_logo_128.png)

  # **Paralax Framework**

  [![.NET 9.0](https://img.shields.io/badge/.NET-9.0-blueviolet?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/9.0)
  [![Build Status](https://github.com/itsharppro/Paralax/actions/workflows/build-test-pack.yml/badge.svg)](https://github.com/itsharppro/Paralax/actions)
  [![NuGet Version](https://img.shields.io/nuget/v/Paralax.svg?style=flat)](https://www.nuget.org/packages/Paralax.Framework/)
  [![Coverage Status](https://codecov.io/gh/itsharppro/Paralax/graph/badge.svg?token=VKEPZNVTOF)](https://codecov.io/gh/itsharppro/Paralax)
  [![License](https://img.shields.io/github/license/itsharppro/Paralax)](https://opensource.org/licenses/Apache-2.0)

</div>

---

## Overview

The **Paralax Framework** is a modern, powerful, and modular open-source .NET framework optimized for building microservice architectures. It provides essential components, including encryption, CQRS, logging, messaging, persistence, cloud integration, and HTTP services. Paralax is optimized for .NET 9.0 and integrates seamlessly into cloud-native environments, making it ideal for scalable and resilient distributed systems.

Whether you’re building microservices from scratch or extending existing services, Paralax offers a comprehensive toolkit that is both flexible and easy to integrate.

---

## Key Features

- **Modular and Extensible**: Tailor your microservices by using only the components you need.
- **Security-First**: Offers encryption, hashing, signing, and secrets management out of the box.
- **Advanced CQRS and Event Sourcing**: Full support for the Command-Query Responsibility Segregation (CQRS) pattern and event sourcing for scalable applications.
- **Message Brokering**: Seamless integration with RabbitMQ, Kafka, and other message brokers.
- **HTTP & Web API**: Robust HTTP client, server support, and API gateway integration.
- **Serialization**: Built-in support for both JSON and binary serialization, crucial for optimized data transfer between microservices.
- **Cloud-Native**: Optimized for Kubernetes, Docker, and other cloud platforms.
- **Observability**: Built-in support for logging, metrics, and distributed tracing using OpenTelemetry and Jaeger.
- **Persistence**: Ready-to-use integrations with MongoDB, Redis, and Consul for robust data storage solutions.
- **API Documentation**: Integration with Swagger and ReDoc to generate comprehensive API documentation.

---

## Table of Contents
- [Getting Started](#getting-started)
- [Installation](#installation)
- [Usage](#usage)
- [Components](#components)
  - [Paralax.Auth](#paralaxauth)
  - [Paralax.CQRS](#paralaxcqrs)
  - [Paralax.MessageBrokers](#paralaxmessagebrokers)
  - [Paralax.HTTP](#paralaxhttp)
  - [Paralax.WebAPI](#paralaxwebapi)
  - [Paralax.Logging](#paralaxlogging)
  - [Paralax.Tracing](#paralaxtracing)
- [Contributing](#contributing)
- [License](#license)
- [Credits](#credits)

---

## Getting Started

### Prerequisites

- .NET 9.0 SDK or higher: [Download .NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
- A container platform like Docker or Kubernetes (for cloud-native applications).
  
### Installation

You can install the framework and its core components via NuGet:

```bash
dotnet add package Paralax
dotnet add package Paralax.Auth
dotnet add package Paralax.CQRS
dotnet add package Paralax.Logging
dotnet add package Paralax.MessageBrokers
```

For specialized functionality HTTP, Web API, and message brokers:

```bash
dotnet add package Paralax.HTTP
dotnet add package Paralax.WebAPI
dotnet add package Paralax.MessageBrokers.RabbitMQ
dotnet add package Paralax.Tracing.Jaeger
```

---

## Usage

Once installed, you can start configuring Paralax in your microservices project. Below is an example of configuring logging, tracing, and CQRS with RabbitMQ for message brokering.

```csharp
public static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Paralax components
    builder.Services.AddParalaxLogging();
    builder.Services.AddParalaxTracing();
    builder.Services.AddParalaxCQRS();
    builder.Services.AddParalaxMessageBrokerRabbitMQ();

    var app = builder.Build();

    // Middleware setup
    app.UseParalax();

    app.Run();
}
```

---



## Components

### Paralax.Auth

The **Paralax.Auth** module provides authentication and authorization mechanisms, including JWT-based authentication and OAuth2 integrations.

### Paralax.CQRS

This module implements the **CQRS** (Command-Query Responsibility Segregation) pattern, allowing for scalable, event-driven microservices. It provides:
- **Commands**: Handling write operations.
- **Queries**: Handling read operations.
- **Events**: Implementing event sourcing.

### Paralax.MessageBrokers

Supports popular messaging systems like **RabbitMQ** and **Kafka**. It enables asynchronous communication between microservices and ensures reliable delivery of messages.

Example:

```csharp
services.AddParalaxMessageBrokerRabbitMQ(options =>
{
    options.HostName = "rabbitmq.example.com";
});
```

### Paralax.HTTP

Provides a robust HTTP client and server integration, supporting **binary serialization** of payloads for performance-critical services.

### Paralax.WebAPI

A framework built on top of **ASP.NET Core Web API**, optimized for building APIs with minimal overhead. It integrates binary serialization for efficient communication and supports **Swagger** for API documentation.

### Paralax.Logging

Out-of-the-box logging integration with support for multiple sinks (e.g., Console, Elasticsearch, etc.). It uses best-in-class logging libraries to ensure visibility into your microservices.

### Paralax.Tracing

Integrated **distributed tracing** with **OpenTelemetry** and **Jaeger** for monitoring service calls and diagnosing bottlenecks in complex microservice architectures.

---

## Contributing

We welcome contributions from the open-source community! To contribute, please refer to the [Contributing Guide](CONTRIBUTING.md) for more details on how to get started.

### Reporting Issues

If you encounter bugs or issues while using Paralax, please report them through the [GitHub Issues](https://github.com/itsharppro/Paralax/issues) page.

---

## License

The **Paralax Framework** is licensed under the **Apache-2.0 License**. See the [LICENSE](LICENSE) file for details.

---

## Credits

The **Paralax Framework** is maintained by **ITSharpPro**. We appreciate contributions from developers around the world who help improve the framework for the global development community.
