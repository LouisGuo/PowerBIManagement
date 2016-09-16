﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VCloud.PowerBIManager
{
    public partial class MainWindow : Form
    {
        private readonly List<ComboBox> workspaceComboBoxs;

        public MainWindow()
        {
            InitializeComponent();
            this.ShowPanel(true);
            this.workspaceComboBoxs = new List<ComboBox>
            {
                comboBox2, comboBox3, comboBox5, comboBox7
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Process(() =>
            {
                this.InitCollections();
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.ShowPanel(true);
            this.Process(() =>
            {
                var collectionName = this.textBox1.Text;
                var accessKey = this.textBox2.Text;
                var powerBIManager = new PowerBIManager(collectionName, accessKey);
                try
                {
                    var collection = powerBIManager.GetWorkspaceCollection();
                    WorkspaceCollectionCache.AddOrUpdate(collection);
                    this.AppendLog(String.Format("Info: add workspace collection successfully. name: {0}", collectionName));
                }
                catch (Exception ex)
                {
                    this.AppendLog(String.Format("Error: add workspace collection failed. name: {0}, details: \n{1}", collectionName, ex));
                }
                InitCollections();
            });
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Process(() =>
            {
                this.LoadWorkspaceCollectionDetail();
            });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Process(() =>
            {
                var currentCollection = this.GetCurrentCollection();
                try
                {
                    var powerBIManager = new PowerBIManager(currentCollection);
                    var result = powerBIManager.CreateWorkspace();
                    this.LoadWorkspaceCollectionDetail();
                    this.AppendLog(String.Format("Info: create new workspace successfully, workspace collection Name: {0}, new workspace id {1}.", currentCollection.Name, result));
                }
                catch (Exception ex)
                {
                    this.AppendLog(String.Format("Error: create new workspace failed, workspace collection Name: {0}, details: \n{1}", currentCollection.Name, ex));
                }
            });
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Process(() =>
            {
                var workspaceId = String.Empty;
                var datasetId = String.Empty;
                this.UIThread(() =>
                {
                    workspaceId = this.comboBox3.SelectedValue.ToString();
                    datasetId = this.comboBox4.SelectedValue.ToString();
                });
                var connStr = this.textBox7.Text;
                var userName = this.textBox8.Text;
                var pwd = this.textBox9.Text;
                var collection = this.GetCurrentCollection();
                if (collection != null)
                {
                    var powerBI = new PowerBIManager(collection);
                    try
                    {
                        powerBI.UpdateConnection(workspaceId, datasetId, connStr, userName, pwd);
                        this.AppendLog("Info: update connection string successfully.");
                    }
                    catch (Exception ex)
                    {
                        this.AppendLog(String.Format("Error: update connection string failed, details: {0}", ex));
                    }
                }
            });
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.Process(() =>
            {
                var workspaceId = String.Empty;
                var datasetId = String.Empty;
                this.UIThread(() =>
                {
                    workspaceId = this.comboBox5.SelectedValue.ToString();
                    datasetId = this.comboBox6.SelectedValue.ToString();
                });
                var collection = this.GetCurrentCollection();
                if (collection != null)
                {
                    var powerBI = new PowerBIManager(collection);
                    try
                    {
                        powerBI.DeleteDataset(workspaceId, datasetId);
                        this.AppendLog("Info: delete dataset successfully.");
                        this.LoadWorkspaceCollectionDetail();
                    }
                    catch (Exception ex)
                    {
                        this.AppendLog(String.Format("Error: delete dataset failed, details: {0}", ex));
                    }
                }
            });
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Process(() =>
            {
                var workspaceId = String.Empty;
                this.UIThread(() =>
                {
                    workspaceId = this.comboBox2.SelectedValue.ToString();
                });
                var datasetName = this.textBox5.Text;
                var filePath = this.label6.Text;
                var collection = this.GetCurrentCollection();
                if (collection != null)
                {
                    var powerBI = new PowerBIManager(collection);
                    var isOverwrite = false;
                    try
                    {
                        using (var fileStream = new FileStream(filePath, FileMode.Open))
                        {
                            powerBI.ImportPbix(workspaceId, datasetName, fileStream, out isOverwrite);
                        }
                        this.AppendLog(String.Format("Info: {0} successfully.", isOverwrite ? "overwrite" : "import"));
                        this.LoadWorkspaceCollectionDetail();
                    }
                    catch (Exception ex)
                    {
                        this.AppendLog(String.Format("Error: {1} failed, details: {0}", ex, isOverwrite ? "overwrite" : "import"));
                    }
                }
            });
        }

        private void button9_Click(object sender, EventArgs e)
        {
            var collection = this.GetCurrentCollection();
            if (collection != null)
            {
                var workspaceId = this.comboBox7.SelectedValue.ToString();
                var reportId = this.comboBox8.SelectedValue.ToString();
                var reportName = this.comboBox8.Text;
                new ReportViewer(collection, workspaceId, reportId, reportName).Show();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            this.label6.Text = this.openFileDialog1.FileName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.ShowPanel(false);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.ShowPanel(true);
        }

        #region private method

        private void SetDatasetSource(ComboBox datasetCombobox, List<DataSet> dataSets)
        {
            this.UIThread(() =>
            {
                var source = new BindingSource();
                source.DataSource = dataSets;
                datasetCombobox.DataSource = source;
                datasetCombobox.ValueMember = "Id";
                datasetCombobox.DisplayMember = "Name";
                source.ResetBindings(true);
            });
        }

        private void SetReportSource(ComboBox reportCombobox, List<Report> reports)
        {
            this.UIThread(() =>
            {
                var source = new BindingSource();
                source.DataSource = reports;
                reportCombobox.DataSource = source;
                reportCombobox.ValueMember = "Id";
                reportCombobox.DisplayMember = "Name";
                source.ResetBindings(true);
            });
        }

        private void AppendLog(String message)
        {
            message += "\n\n\n\n";
            this.richTextBox2.UIThread(() =>
            {
                this.richTextBox2.AppendText(message);
                this.richTextBox2.SelectionStart = this.richTextBox2.TextLength;
                this.richTextBox2.ScrollToCaret();
            });
            FileHelper.AppendLog(message);
        }

        private void Process(Action action)
        {
            var totalTime = new TimeSpan(1000);
            var tempLabel = new Label();
            this.panel3.Controls.Add(tempLabel);
            var actionThread = new Thread(() =>
            {
                action();
                this.UIThread(() =>
                {
                    tempLabel.Text = "ready";
                    Thread.Sleep(1000);
                    this.panel3.Controls.Remove(tempLabel);
                });
            });
            actionThread.Start();
            var timerThread = new Thread(() =>
            {
                while (!tempLabel.Equals("ready") && this.panel3.Controls.Contains(tempLabel))
                {
                    totalTime += new TimeSpan(0, 0, 1);
                    this.UIThread(() =>
                    {
                        tempLabel.Text = totalTime.ToString("hh\\:mm\\:ss");
                    });
                    Thread.Sleep(1000);
                }
            });
            timerThread.Start();
        }

        private void Panel3LabelChanged(object sender, ControlEventArgs e)
        {
            var panel = sender as Panel;
            for (int i = 0; i < panel.Controls.Count; i++)
            {
                panel.Controls[i].Left = 100 * i;
            }
        }

        private void ShowPanel(Boolean showList)
        {
            this.UIThread(() =>
            {
                this.panel1.Visible = showList;
                this.panel2.Visible = !showList;
                this.splitContainer1.Visible = showList;
                this.BackColor = showList ? Control.DefaultBackColor : Color.White;
                this.panel2.BackColor = Control.DefaultBackColor;
            });
        }

        private void InitCollections()
        {
            this.comboBox1.UIThread(() =>
            {
                var source = new BindingSource();
                source.DataSource = WorkspaceCollectionCache.Cache;
                this.comboBox1.ValueMember = "AccessKey";
                this.comboBox1.DisplayMember = "Name";
                this.comboBox1.DataSource = source;
                source.ResetBindings(true);

            });
        }

        private void LoadWorkspaceCollectionDetail()
        {
            var currentCollection = GetCurrentCollection();
            if (currentCollection != null)
            {
                this.UIThread(() =>
                {
                    this.label16.Text = currentCollection.Name;
                    this.richTextBox_WorkspaceCollection.Text = "...";
                    this.splitContainer1.Enabled = false;
                });
                try
                {
                    var powerBIManager = new PowerBIManager(currentCollection);
                    var collection = powerBIManager.GetWorkspaceCollection();
                    WorkspaceCollectionCache.AddOrUpdate(collection);
                    var str = JsonSerializer.ConvertObjToString(collection);
                    this.UIThread(() =>
                    {
                        this.richTextBox_WorkspaceCollection.Text = str;
                        this.splitContainer1.Enabled = true;
                    });
                    var source = new BindingSource();
                    source.DataSource = collection.Workspaces;
                    foreach (var combo in workspaceComboBoxs)
                    {
                        this.UIThread(() =>
                        {
                            combo.DataSource = source;
                            combo.ValueMember = "Id";
                            combo.DisplayMember = "Id";
                        });
                    }
                    source.ResetBindings(true);
                    this.AppendLog(String.Format("Info: load collection details successfully, collectionName: {0}", currentCollection.Name));
                }
                catch (Exception ex)
                {
                    this.AppendLog(String.Format("Error: load collection details failed, collectionName: {0}, details: {1}", currentCollection.Name, ex));
                }
            }

        }

        private WorkspaceCollection GetCurrentCollection()
        {
            var source = (BindingSource)this.comboBox1.DataSource;
            var dataSource = (List<WorkspaceCollection>)source.DataSource;
            if (dataSource != null && dataSource.Count > 0)
            {
                var index = 0;
                this.UIThread(() =>
                {
                    index = this.comboBox1.SelectedIndex;
                });
                return dataSource[index];
            }
            return null;
        }

        #endregion private method

        private void comboBox3_SelectedIndexChanged(Object sender, EventArgs e)
        {
            var collection = this.GetCurrentCollection();
            var datasets = new List<DataSet>();
            if (collection != null && collection.Workspaces.Count > 0)
            {
                datasets = collection.Workspaces[this.comboBox3.SelectedIndex].DataSets;
            }
            this.SetDatasetSource(this.comboBox4, datasets);
        }

        private void comboBox5_SelectedIndexChanged(Object sender, EventArgs e)
        {
            var collection = this.GetCurrentCollection();
            var datasets = new List<DataSet>();
            if (collection != null && collection.Workspaces.Count > 0)
            {
                datasets = collection.Workspaces[this.comboBox5.SelectedIndex].DataSets;
            }
            this.SetDatasetSource(this.comboBox6, datasets);
        }

        private void comboBox7_SelectedIndexChanged(Object sender, EventArgs e)
        {
            var collection = this.GetCurrentCollection();
            var reports = new List<Report>();
            if (collection != null && collection.Workspaces.Count > 0)
            {
                reports = collection.Workspaces[this.comboBox7.SelectedIndex].Reports;
            }
            this.SetReportSource(this.comboBox8, reports);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                var common = AzureADHelper.GetCommonAzureAccessToken();
                this.ShowPanel(true);
                this.AppendLog("Info: office 365 login successfully.");
                this.Process(() =>
                {
                    try
                    {
                        var accessToken = AzureADHelper.GetAzureAccessToken(common);
                        var subscriptionId = AzureADHelper.GetSubscriptionId(common);
                        var powerBIApiHelper = new PowerBIApiHelper(accessToken, subscriptionId);
                        var collections = powerBIApiHelper.GetWorkspaceCollections();
                        var result = new List<WorkspaceCollection>();
                        foreach (var item in collections)
                        {
                            var accessKey = powerBIApiHelper.ListWorkspaceCollectionKey(item.Item1, item.Item2);
                            var temp = new WorkspaceCollection
                            {
                                Name = item.Item2,
                                AccessKey = accessKey
                            };
                            result.Add(temp);
                        }
                        WorkspaceCollectionCache.AddRange(result);
                        this.AppendLog("Info: get all workspace collections successfully.");
                        this.InitCollections();
                    }
                    catch (Exception exception)
                    {
                        this.AppendLog(String.Format("Error: exception happened when invoking office 365 api , details: {0}", exception));
                    }
                });

            }
            catch (Exception ex)
            {
                this.AppendLog(String.Format("Error: office 365 login exception happend, details: {0}", ex));
            }
        }
    }
}
