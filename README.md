# Carsties Auctions Platform

## Project Overview

Carsties is a distributed auction platform for cars, comprising multiple microservices that handle different aspects of the application, such as auction management, bidding, search, identity, and notifications. This README provides comprehensive documentation for developers, including setup, usage, and contribution guidelines.

## Features and Functionality

*   **Auction Creation & Management:** Sellers can create and manage auction listings.
*   **Bidding System:** Registered users can place bids on active auctions.
*   **Search Functionality:** Users can search for cars based on make, model, color, and other criteria.
*   **User Authentication & Authorization:** Secure user registration, login, and authorization using IdentityServer6.
*   **Real-time Notifications:**  Updates on auction events (bid placement, auction ending) are delivered via SignalR.

## Technology Stack

The Carsties platform is built using the following technologies:

*   **C# .NET:** Backend microservices are developed using C# and the .NET framework.
*   **Next.js (React):** The frontend web application is built using Next.js and React.
*   **MassTransit:** Message queue implementation using RabbitMQ for inter-service communication.
*   **MongoDB:**  Used in Bidding and Search services for data storage.
*   **PostgreSQL:** Used in Auction and Identity services for data storage.
*   **AutoMapper:**  Library used for object-object mapping between different layers.
*   **IdentityServer6:** Provides authentication and authorization services.
*   **SignalR:** Enables real-time communication between backend and frontend.
*   **gRPC:** Used for inter-service communication, specifically between Bidding Service and Auction Service.
*   **Polly:** Resilience framework to handle transient errors.
*   **Flowbite-React:** A UI component library using React and Flowbite.
*   **react-hook-form:** For handling forms in React.
*   **NextAuth.js:** For authentication in Next.js application.
*   **Zustand:** For state management in React.
*   **query-string:** For URL Query parameters parsing and stringifying.

## Prerequisites

Before setting up the Carsties platform, ensure the following prerequisites are installed:

*   .NET SDK 8.0 or later
*   Node.js and npm
*   Docker (recommended for RabbitMQ and MongoDB deployment)
*   PostgreSQL
*   Duende IdentityServer

## Installation Instructions

This section outlines the steps to install and configure each microservice within the Carsties platform.

### 1. Infrastructure Setup (RabbitMQ and MongoDB)

It is recommended to use Docker for setting up RabbitMQ and MongoDB.

*   **RabbitMQ:**

    ```bash
    docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
    ```

*   **MongoDB:**

    ```bash
    docker run -d --name mongodb -p 27017:27017 -e MONGO_INITDB_ROOT_USERNAME=root -e MONGO_INITDB_ROOT_PASSWORD=secret mongo
    ```

### 2. Backend Services

Each backend service should be setup and run separately.

#### 2.1 AuctionService

*   Navigate to the `src/AuctionService` directory.
*   Update the `appsettings.Development.json` file with the correct connection strings for PostgreSQL and RabbitMQ. Example:

    ```json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "RabbitMq": {
        "Host": "localhost",
        "Username": "guest",
        "Password": "guest"
      },
      "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Database=AuctionServiceDb;Username=postgres;Password=secret"
      },
      "IdentityServiceUrl": "http://localhost:5000"
    }
    ```

*   Apply database migrations:

    ```bash
    dotnet ef database update -p AuctionService -s AuctionService
    ```

*   Run the service:

    ```bash
    dotnet run -p AuctionService
    ```

#### 2.2 BiddingService

*   Navigate to the `src/BiddingService` directory.
*   Update the `appsettings.Development.json` file with the correct connection strings for MongoDB and RabbitMQ. Example:

    ```json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "RabbitMq": {
        "Host": "localhost",
        "Username": "guest",
        "Password": "guest"
      },
      "ConnectionStrings": {
        "BidDbConnection": "mongodb://root:secret@localhost:27017"
      },
      "IdentityServiceUrl": "http://localhost:5000",
      "GrpcAuction": "http://localhost:5002"
    }
    ```

*   Run the service:

    ```bash
    dotnet run -p BiddingService
    ```

#### 2.3 NotificationService

*   Navigate to the `src/NotificationService` directory.
*   Update the `appsettings.Development.json` file with the correct connection strings for RabbitMQ. Example:

    ```json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "RabbitMq": {
        "Host": "localhost",
        "Username": "guest",
        "Password": "guest"
      }
    }
    ```

*   Run the service:

    ```bash
    dotnet run -p NotificationService
    ```

#### 2.4 SearchService

*   Navigate to the `src/SearchService` directory.
*   Update the `appsettings.Development.json` file with the correct connection strings for MongoDB and RabbitMQ. Example:

    ```json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "MongoDbConnection": "mongodb://root:secret@localhost:27017",
      "AuctionServiceUrl": "http://localhost:5002",
      "RabbitMq": {
        "Host": "localhost",
        "Username": "guest",
        "Password": "guest"
      }
    }
    ```

*   Run the service:

    ```bash
    dotnet run -p SearchService
    ```

#### 2.5 GatewayService

*   Navigate to the `src/GatewayService` directory.
*   Update the `appsettings.Development.json` file with the correct service URLs.  Example:

    ```json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "IdentityServiceUrl": "http://localhost:5000",
      "ClientApp": "http://localhost:3000",
      "ReverseProxy": {
          "Routes": {
              "auctionsRoute": {
                  "ClusterId": "auctionsCluster",
                  "Match": {
                      "Path": "/api/auctions/{**catch-all}"
                  }
              },
              "bidsRoute": {
                  "ClusterId": "bidsCluster",
                  "Match": {
                      "Path": "/api/bids/{**catch-all}"
                  }
              },
               "searchRoute": {
                  "ClusterId": "searchCluster",
                  "Match": {
                      "Path": "/api/search/{**catch-all}"
                  }
              }
          },
          "Clusters": {
              "auctionsCluster": {
                  "LoadBalancingPolicy": "RoundRobin",
                  "Destinations": {
                      "auctionsDestination": {
                          "Address": "http://localhost:5002"
                      }
                  }
              },
              "bidsCluster": {
                  "LoadBalancingPolicy": "RoundRobin",
                  "Destinations": {
                      "bidsDestination": {
                          "Address": "http://localhost:5003"
                      }
                  }
              },
               "searchCluster": {
                  "LoadBalancingPolicy": "RoundRobin",
                  "Destinations": {
                      "searchDestination": {
                          "Address": "http://localhost:5004"
                      }
                  }
              }
          }
      }
    }
    ```

*   Run the service:

    ```bash
    dotnet run -p GatewayService
    ```

#### 2.6 IdentityService

*   Navigate to the `src/IdentityService` directory.
*   Update the `appsettings.Development.json` file with the correct connection strings for PostgreSQL and client application URL. Example:

    ```json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Database=IdentityServiceDb;Username=postgres;Password=secret"
      },
      "ClientApp": "http://localhost:3000",
      "IssuerUri": "http://localhost:5000"
    }
    ```

*   Apply database migrations:

    ```bash
    dotnet ef database update -p IdentityService -s IdentityService
    ```

*   Seed initial data:

    ```bash
    dotnet run SeedData
    ```

*   Run the service:

    ```bash
    dotnet run -p IdentityService
    ```

### 3. Frontend Application

*   Navigate to the `frontend/web-app` directory.
*   Install dependencies:

    ```bash
    npm install
    ```

*   Update the `.env.local` file with the correct API URLs:

    ```
    API_URL=http://localhost:7000/api
    NEXT_PUBLIC_NOTIFY_URL=http://localhost:5005/notifications
    ID_URL=http://localhost:5000
    ```

*   Run the application:

    ```bash
    npm run dev
    ```

## Usage Guide

1.  **Access the Frontend:** Open your web browser and navigate to `http://localhost:3000`.
2.  **Authentication:** Use the IdentityService UI to register a new user or log in with an existing account.
3.  **Browsing Auctions:** View a list of active auctions on the homepage.
4.  **Creating Auctions:**  Log in and navigate to the "Sell my car" section to create a new auction.
5.  **Placing Bids:**  Navigate to an auction detail page and place a bid.
6.  **Search:** Use the search bar in the navigation to search auctions by make, model, or color.
7.  **Real-time updates:** Auction and bid events are displayed in real-time.

## API Documentation

The Carsties platform exposes several REST APIs through the Gateway Service. All endpoints are prefixed with `/api`.

### Auctions Service API

*   `GET /api/auctions`: Retrieves a list of auctions.
*   `GET /api/auctions/{id}`: Retrieves a specific auction by ID.
*   `POST /api/auctions`: Creates a new auction. Requires authentication.
*   `PUT /api/auctions/{id}`: Updates an existing auction. Requires authentication and authorization (seller only).
*   `DELETE /api/auctions/{id}`: Deletes an auction. Requires authentication and authorization (seller only).

### Bidding Service API

*   `POST /api/bids?auctionId={auctionId}&amount={amount}`: Places a bid on an auction. Requires authentication.
*   `GET /api/bids/{auctionId}`: Retrieves all bids for a specific auction.

### Search Service API

*   `GET /api/search`: Searches for items based on various criteria (searchTerm, pageNumber, pageSize, orderBy, filterBy).

## Contributing Guidelines

We welcome contributions to the Carsties platform. Please follow these guidelines:

1.  **Fork the Repository:** Fork the main repository to your GitHub account.
2.  **Create a Branch:** Create a new branch for your feature or bug fix.
3.  **Code Style:**  Follow the existing coding style and conventions used in the project.
4.  **Commit Messages:** Write clear and concise commit messages.
5.  **Pull Request:** Submit a pull request to the main repository.  Include a detailed description of the changes and the problem it addresses.

## License Information

This project does not specify a license. All rights are reserved unless otherwise stated.

## Contact/Support Information

For questions, bug reports, or feature requests, please contact:

*   Mohammed Zrirake - <[mohammed.zrirake01@gmail.com]>
*   [GitHub Repo Link](https://github.com/Mohammed-Zrirake/Carsties)