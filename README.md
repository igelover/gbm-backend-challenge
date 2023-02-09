# GBM Software Engineer Challenge

This repository contains a solution developed as part of the GBM Software Engineer Challenge.
Please refer to the [instructions and requirements](./docs/Back%20End%20Challenge.pdf) for better reference.

## Overview

The solution implements a [Clean Architecture](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture) using [CQRS Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs), [MediatR](https://github.com/jbogard/MediatR), [Repository Pattern with Entity Framework](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-implementation-entity-framework-core) and [Options Pattern](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options), this will have the benefit of easier maintainability and evolution of the app.

## Implementation Highlights

In the _Duplicated Operation_ business rule, the requirement is not explicit about the comparison of the Operation Type, however it's being included in the current implementation, as a buy operation cannot be considered duplicate with a sell operation.

In the _Closed Market_ business rule, there's no explicit reference to any specific timezone, so no timezone is currently being used for comparison.

In case of multiple operations with the same issuer name, the issuer information is being repeated, probably they should be grouped, however the requirement is not clear about what groupig criteria to use. The problematic field would be _share_price_, as we can have operations with the same issuer name but different share price. A business decision is required here.

Regarding the JSON serialization, a couple of custom policies are being used. One in order to comply with the snake case specification, and the other to automatically convert to uppercase the enums serialization. 

Also, a custom JSON converter was introduced in order to parse UNIX timestamps into DateTime instances at binding time, and a custom Epoch was implemented as per the [UNIX timestamp specification](https://unixtime.org/).

## Prerequisites

## Build and Run

## Potential Enhancements

* Add a stock catalog to have a list of valid issuer names and verify at order creation time.
* Add a business operation timezone to the app seetings to use on the _Closed Market_ business rule.
* In the _current_balance_ node, group the _issuers_ by _issuer_name_ and calculate the weighted average price per share.