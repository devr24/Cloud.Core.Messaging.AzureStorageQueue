# **Cloud.Core.Messaging.AzureStorageQueue** 
[![Build status](https://dev.azure.com/cloudcoreproject/CloudCore/_apis/build/status/Cloud.Core%20Packages/Cloud.Core.Messenging.AzureQueueStorage_Package)](https://dev.azure.com/cloudcoreproject/CloudCore/_build/latest?definitionId=20) 
![Code Coverage](https://cloud1core.blob.core.windows.net/codecoveragebadges/Cloud.Core.Messaging.AzureStorageQueue-LineCoverage.png) 
[![Cloud.Core.Messaging.AzureStorageQueue package in Cloud.Core feed in Azure Artifacts](https://feeds.dev.azure.com/cloudcoreproject/dfc5e3d0-a562-46fe-8070-7901ac8e64a0/_apis/public/Packaging/Feeds/8949198b-5c74-42af-9d30-e8c462acada6/Packages/590eaaba-691a-4488-accd-682c039c0553/Badge)](https://dev.azure.com/cloudcoreproject/CloudCore/_packaging?_a=package&feed=8949198b-5c74-42af-9d30-e8c462acada6&package=590eaaba-691a-4488-accd-682c039c0553&preferRelease=true)

<div id="description">

Azure specific implementation of queue storage interface.  Uses the _IReactiveMessenger_ or _IMessenger_ interface from _Cloud.Core_.

</div>

# **Usage**

## **Initialisation and Authentication Usage**

There are three ways you can instantiate the Queue Storage Client.  Each way dictates the security mechanism the client uses to connect.  The three mechanisms are:

1. Connection String
2. Service Principle
3. Managed Service Identity

Below are examples of instantiating each type.

#### 1. Connection String
Create an instance of the Queue Storage client using a connection string as follows:

```csharp
var connConfig = new ConnectionConfig
    {
        ConnectionString = "<connectionstring>",
        ReceiverSetup = new ReceiverSetup { ... }, 
        SenderSetup = new SenderSetup { ... }
    };

// Queue storage client.
var queuestorage = new QueueStorage(connConfig);		
```
Note: Instance name not required to be specified anywhere in configuration here as it is taken from the connection string itself.

#### 2. Service Principle
Create an instance of the Queue Storage client with Service Principle authentication as follows:

```csharp
var spConfig = new ServicePrincipleConfig
    {
        AppId = "<appid>",
        AppSecret = "<appsecret>",
        TenantId = "<tenantid>",
        InstanceName = "<queueinstancename>",
        SubscriptionId = "<subscriptionId>",
        ReceiverSetup = new ReceiverSetup { ... }, 
        SenderSetup = new SenderSetup { ... }
    };

// Queue storage client.
var queuestorage = new QueueStorage(spConfig);	
```


#### 3. Management Service Idenity (MSI) 
This authentication also works for Managed User Identity.  Create an instance of the Queue Storage client with MSI authentication as follows:

```csharp
var msiConfig = new MsiConfig
    {
        TenantId = "<tenantid>",
        InstanceName = "<queueinstancename>",
        SubscriptionId = "<subscriptionId>",
        ReceiverSetup = new ReceiverSetup { ... }, 
        SenderSetup = new SenderSetup { ... }
    };

// Queue storage client.
var queuestorage = new QueueStorage(msiConfig);	
```

All that's required is the instance name, tenantId and subscriptionId to connect to.  Authentication runs under the context the application is running.

## Dependency Injection

Inserting into dependency container:

```csharp
// Add multiple instances of state storage.
services.AddStorageQueueSingletonNamed<IReactiveMessenger>("QM1", "queueStorageInstanceName", "tenantId", "subscriptionId",
        ReceiverSetup = new ReceiverSetup { ... }, 
        SenderSetup = new SenderSetup { ... }); 

// add to factory using a key
services.AddStorageQueueSingletonNamed<IReactiveMessenger>("QM2", "queueStorageInstanceName2", "tenantId", "subscriptionId",
        ReceiverSetup = new ReceiverSetup { ... }, 
        SenderSetup = new SenderSetup { ... }); 

// add to factory using a key
serviceCollection.AddQueueStorageSingleton<IMessenger>("tableStorageInstance3", "tenantId", "subscriptionId",
        ReceiverSetup = new ReceiverSetup { ... }, 
        SenderSetup = new SenderSetup { ... });    // add to factory using instance name

// Sample consuming class.
services.AddTransient<MyClass>();
```

Using the dependencies:

```csharp
public class MyClass {

	private readonly IReactiveMessenger _messageInstance1;
	private readonly IReactiveMessenger _messageInstance2;
	private readonly IMessenger _messageInstance3;

	public MyClass(NamedInstanceFactory<IReactiveMessenger> messengerFactor, IMessenger singleMessengerInstance) 
	{	
		_messageInstance1 = messengerFactor["QM1"];
		_messageInstance2 = messengerFactor["QM2"];
		_messageInstance3 = singleMessengerInstance;
	}
	
	...
}
```



## Test Coverage
A threshold will be added to this package to ensure the test coverage is above 80% for branches, functions and lines.  If it's not above the required threshold 
(threshold that will be implemented on ALL of the core repositories to gurantee a satisfactory level of testing), then the build will fail.

## Compatibility
This package has has been written in .net Standard and can be therefore be referenced from a .net Core or .net Framework application. The advantage of utilising from a .net Core application, 
is that it can be deployed and run on a number of host operating systems, such as Windows, Linux or OSX.  Unlike referencing from the a .net Framework application, which can only run on 
Windows (or Linux using Mono).
 
## Setup
This package is built using .net Standard 2.1 and requires the .net Core 3.1 SDK, it can be downloaded here: 
https://www.microsoft.com/net/download/dotnet-core/

IDE of Visual Studio or Visual Studio Code, can be downloaded here:
https://visualstudio.microsoft.com/downloads/

## How to access this package
All of the Cloud.Core.* packages are published to a internal NuGet feed.  To consume this on your local development machine, please add the following feed to your feed sources in Visual Studio:
https://pkgs.dev.azure.com/cloudcoreproject/CloudCore/_packaging/Cloud.Core/nuget/v3/index.json
 
For help setting up, follow this article: https://docs.microsoft.com/en-us/vsts/package/nuget/consume?view=vsts


<img src="https://cloud1core.blob.core.windows.net/icons/cloud_core_small.PNG" />
