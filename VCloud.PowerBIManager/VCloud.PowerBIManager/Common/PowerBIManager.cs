using Microsoft.PowerBI.Api.V1;
using Microsoft.PowerBI.Api.V1.Models;
using Microsoft.PowerBI.Security;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VCloud.PowerBIManager
{
    class PowerBIManager
    {
        private readonly String workspaceCollectionName;

        private readonly String accessKey;

        public PowerBIManager(String workspaceCollectionName, String accessKey)
        {
            this.workspaceCollectionName = workspaceCollectionName;
            this.accessKey = accessKey;
        }

        public PowerBIManager(WorkspaceCollection collection)
        {
            this.workspaceCollectionName = collection.Name;
            this.accessKey = collection.AccessKey;
        }

        public String CreateWorkspace()
        {
            using (var client = CreateClient())
            {
                return client.Workspaces.PostWorkspace(this.workspaceCollectionName).WorkspaceId;
            }
        }

        public void UpdateConnection(String workspaceId, String datasetId, String connectionString, String username, String password)
        {
            using (var client = CreateClient())
            { 
                var connectionParameters = new Dictionary<String, Object>
                {
                    { "connectionString", connectionString }
                };
                client.Datasets.SetAllConnections(workspaceCollectionName, workspaceId, datasetId, connectionParameters);
                var datasources = client.Datasets.GetGatewayDatasources(workspaceCollectionName, workspaceId, datasetId);
                var delta = new GatewayDatasource
                {
                    CredentialType = "Basic",
                    BasicCredentials = new BasicCredentials
                    {
                        Username = username,
                        Password = password
                    }
                };
                client.Gateways.PatchDatasource(workspaceCollectionName, workspaceId,
                  datasources.Value[datasources.Value.Count - 1].GatewayId,
                  datasources.Value[datasources.Value.Count - 1].Id, delta);
            }
        }

        public void DeleteDataset(String workspaceId, String datasetId)
        {
            using (var client = CreateClient())
            {
                client.Datasets.DeleteDatasetById(workspaceCollectionName, workspaceId, datasetId);
            }
        }

        public Boolean ImportPbix(String workspaceId, String datasetName, Stream fileStream, out Boolean isOverwrite)
        {
            using (var client = CreateClient())
            {
                isOverwrite = false;
                client.HttpClient.Timeout = TimeSpan.FromMinutes(60);
                client.HttpClient.DefaultRequestHeaders.Add("ActivityId", Guid.NewGuid().ToString());
                var datasets = this.GetDataSets(this.workspaceCollectionName, workspaceId);
                String nameConflict = null;
                if (datasets.FirstOrDefault(d => d.Name.Equals(datasetName)) != null)
                {
                    isOverwrite = false;
                    nameConflict = "Overwrite";
                }
                var import = client.Imports.PostImportWithFile(workspaceCollectionName, workspaceId, fileStream, datasetName, nameConflict);
                while (import.ImportState != "Succeeded" && import.ImportState != "Failed")
                {
                    import = client.Imports.GetImportById(workspaceCollectionName, workspaceId, import.Id);
                    Thread.Sleep(1000);
                }
                return import.ImportState == "Succeeded";
            }
        }

        public WorkspaceCollection GetWorkspaceCollection()
        {
            using (var client = CreateClient())
            {
                var result = new WorkspaceCollection();
                result.Name = this.workspaceCollectionName;
                result.AccessKey = this.accessKey;
                result.Workspaces = this.GetWorkspaces(this.workspaceCollectionName);
                return result;
            }
        }

        public String GetAccessToken(String workspaceId, String reportId)
        {
            using (var client = CreateClient())
            {
                var embedToken = PowerBIToken.CreateReportEmbedToken(this.workspaceCollectionName, workspaceId, reportId);
                return embedToken.Generate(this.accessKey);
            }
        }

        private List<Workspace> GetWorkspaces(String workspaceCollectionName)
        {
            using (var client = CreateClient())
            {
                var response = client.Workspaces.GetWorkspacesByCollectionName(workspaceCollectionName);
                var result = new List<Workspace>();
                foreach (var workspace in response.Value)
                {
                    var temp = new Workspace();
                    temp.Id = workspace.WorkspaceId;
                    temp.Reports = this.GetReports(workspaceCollectionName, workspace.WorkspaceId);
                    temp.DataSets = this.GetDataSets(workspaceCollectionName, workspace.WorkspaceId);
                    result.Add(temp);
                }
                return result;
            }
        }

        private List<Report> GetReports(String workspaceCollectionName, String workspaceId)
        {
            using (var client = CreateClient())
            {
                var response = client.Reports.GetReports(workspaceCollectionName, workspaceId);
                var result = new List<Report>();
                foreach (var report in response.Value)
                {
                    var temp = new Report();
                    temp.Id = report.Id;
                    temp.Name = report.Name;
                    temp.EmbedUrl = report.EmbedUrl;
                    temp.WebUrl = report.WebUrl;
                    result.Add(temp);
                }
                return result;
            }
        }

        private List<DataSet> GetDataSets(String workspaceCollectionName, String workspaceId)
        {
            using (var client = CreateClient())
            {
                var response = client.Datasets.GetDatasets(workspaceCollectionName, workspaceId);
                var result = new List<DataSet>();
                foreach (var dataset in response.Value)
                {
                    var temp = new DataSet();
                    temp.Id = dataset.Id;
                    temp.Name = dataset.Name;
                    result.Add(temp);
                }
                return result;
            }
        }

        private PowerBIClient CreateClient()
        {
            var credentials = new TokenCredentials(accessKey, "AppKey");
            var client = new PowerBIClient(credentials);
            client.BaseUri = new Uri("https://api.powerbi.com");
            return client;
        }
    }
}
