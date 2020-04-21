# **Cloud.Core.Messaging.AzureStorageQueue** 
[![Build status](https://dev.azure.com/cloudcoreproject/CloudCore/_apis/build/status/Cloud.Core/Cloud.Core.Configuration_Package)](https://dev.azure.com/cloudcoreproject/CloudCore/_build/latest?definitionId=6) 
![Code Coverage](https://cloud1core.blob.core.windows.net/codecoveragebadges/Cloud.Core.Messaging.AzureStorageQueue-LineCoverage.png) 
[![Cloud.Core.Configuration package in Cloud.Core feed in Azure Artifacts](https://feeds.dev.azure.com/cloudcoreproject/dfc5e3d0-a562-46fe-8070-7901ac8e64a0/_apis/public/Packaging/Feeds/8949198b-5c74-42af-9d30-e8c462acada6/Packages/e71ddf20-f66a-45da-b672-c32798cf1e51/Badge)](https://dev.azure.com/cloudcoreproject/CloudCore/_packaging?_a=package&feed=8949198b-5c74-42af-9d30-e8c462acada6&package=e71ddf20-f66a-45da-b672-c32798cf1e51&preferRelease=true)


---------------

<div id="description">

A lightweight adapter for Azure Service Bus (Queues and Topics), that abstracts consuming applications from the Service Bus technology stack by 
implementing common messaging interfaces.

</div>

## Design


## Usage



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