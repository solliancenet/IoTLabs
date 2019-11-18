# Anomaly Detection

## Prerequisites

### Technical Requirements

Attendees will need to bring their own laptops and have access to an Azure subscription that can accommodate the following:

- An Azure Machine Learning workspace - Basic edition (instructions on how to provision are available at https://docs.microsoft.com/en-us/azure/machine-learning/service/how-to-manage-workspace). It is recommended to create the workspace in region: **East US**.

- An Azure Machine Learning Notebook VM with a recommended size of Standard_D3_v2 (details on VM sizing are available at https://docs.microsoft.com/en-us/azure/cloud-services/cloud-services-sizes-specs#dv2-series).

- An Azure Machine Learning compute cluster with one node with a recommended size of Standard_NC6 (details on GPU VM sizing are available at https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-gpu#nc-series). Note that not all regions support creation of the Standard_NC6 training cluster and thus it is recommended to create your machine learning workspace in region `East US`. If you create your workspace in a different region, please confirm that the region support creation of Standard_NC6 training cluster.

- An Azure Container Instances instance.

Please note that all of the above (assuming recommended sizes) amount to 10 VM vCores. Please make sure your subscription's quota has available at least this number of vCores.

It is recommended that you create the Azure Machine Learning workspace (**Workspace edition: Basic, Region: East US**) and the Notebook VM prior to starting the lab. The Azure Machine Learning compute cluster and the ACI instance will be created during the lab. Details on how to create and configure your Notebook VM are shown below.

### Requirements for real-time predictions lab

In Lab 3, you will deploy a trained machine learning model. After this step, you will create a stream processing workflow that calls the trained ML model in real time to detect anomalies in incoming sensor telemetry. To complete this portion of the lab, you need the following:

- [Visual Studio 2019 community](https://visualstudio.microsoft.com/vs/community/)
- [Visual Studio **.NET Desktop Development**, **ASP.NET and Web Development**, and **Azure Development** Workloads](https://docs.microsoft.com/en-us/visualstudio/install/modify-visual-studio?view=vs-2019)
- [.NET Core 3.0 **SDK**](https://dotnet.microsoft.com/download/dotnet-core/3.0)

### Azure Notebook VMs Setup

Please follow instructions outlined in [Azure Notebook VMs Setup](./azure-notebook-vms-setup) to create and update your Azure Notebook VM, and download the lab notebooks prior to starting the lab.

### Begin the Day 2 labs

Begin the Day 2 labs by following the instructions in the [`LabsDocumentation` folder](LabsDocumentation/README.md).
