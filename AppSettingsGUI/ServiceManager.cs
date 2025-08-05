﻿#region Copyright
//  Copyright 2016 Patrice Thivierge F.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewApp.Core.Helpers;

namespace NewApp.Settings.GUI
{
    public partial class ServiceManager : Form
    {
        private readonly ILogger<ServiceManager> _logger;
        private readonly IConfiguration _configuration;
        private XmlSettingsManager _currentSetting;

        public ServiceManager(ILogger<ServiceManager> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        InitializeComponent();
    }

    // Parameterless constructor for designer support
    public ServiceManager() : this(null!, null!)
    {
        // This constructor should only be used by the designer
        // In runtime, use the DI constructor
    }

        private void InitTreeView()
        {
            treeView1.Select();

            object setting;
            var root = new TreeNode("Settings");
            treeView1.Nodes.Add(root);

            string appSettingsPath = Settings.Default.SettingsPath;
            string settingsCommentsPath = Settings.Default.SettingsCommentsPath;

            if (!Path.IsPathRooted(appSettingsPath))
                appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, appSettingsPath);

            if (!Path.IsPathRooted(settingsCommentsPath))
                settingsCommentsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settingsCommentsPath);


            _logger?.LogDebug("Will try to load service settings file: {AppSettingsPath}", appSettingsPath);
            _logger?.LogDebug("Will try to load comments file: {SettingsCommentsPath}", settingsCommentsPath);

            // here is the place you have to add your setting files
            setting = new XmlSettingsManager(typeof (General).FullName, appSettingsPath, settingsCommentsPath);
            root.Nodes.Add(new TreeNode("General Settings") {Tag = setting});

            setting = new XmlSettingsManager(typeof (Advanced).FullName, appSettingsPath, settingsCommentsPath);
            root.Nodes.Add(new TreeNode("Advanced Settings") {Tag = setting});

            //continue to add your settings categories here...

            // end of add settings files

            treeView1.ExpandAll();
            treeView1.SelectedNode = root.FirstNode;

            treeView1_NodeMouseClick(this, new TreeNodeMouseClickEventArgs(root.FirstNode, MouseButtons.Right, 1, 0, 0));
        }

        private void SettingsManager_Load(object sender, EventArgs e)
        {
            try
            {
                _logger?.LogDebug("Application Started.");
                serviceController1.ServiceName = Settings.Default.ServiceName;
                serviceController1.MachineName = System.Environment.MachineName;
                Init();
                timer1.Start();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during application startup");
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(serviceController1.ServiceName))
                    serviceController1.ServiceName = Settings.Default.ServiceName;

                serviceController1.Refresh();
                
                lblServiceName.Text = serviceController1.ServiceName;
                lblServiceState.Text = serviceController1.Status.ToString();

                switch (lblServiceState.Text)
                {
                    case "Running":
                        btnServiceAction.Text = "Stop";
                        break;
                    case "Stopped":
                        btnServiceAction.Text = "Start";
                        break;
                }

                btnServiceAction.Enabled = true;
            }
            catch (Exception ex)
            {
                lblServiceState.Text = "Error while looking for service.";
                btnServiceAction.Enabled = false;
                //_logger?.LogError(ex, "Timer tick error"); // this would fill up logs... 
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Save();
            toolStripStatusLabel1.Text = "Settings saved at " + DateTime.Now;
        }

        private void Save()
        {
            ((XmlSettingsManager) propertyGrid1.SelectedObject).SaveSettings();
        }


        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                object settings = e.Node.Tag;

                propertyGrid1.SelectedObject = settings;
                _currentSetting = (XmlSettingsManager) propertyGrid1.SelectedObject;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            switch (btnServiceAction.Text)
            {
                case "Start":
                    if (serviceController1.Status != ServiceControllerStatus.StartPending &&
                        serviceController1.Status != ServiceControllerStatus.Running)
                        serviceController1.Start();
                    break;
                case "Stop":
                    if (serviceController1.CanStop)
                        serviceController1.Stop();
                    break;
            }
        }

        private void Init()
        {
            _logger?.LogDebug("Initializing application.");
            InitTreeView();

            // ajoute un tooltip au bouton save
            // Create the ToolTip and associate with the Form container.
            var toolTip1 = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                ShowAlways = true
            };

            // Set up the delays for the ToolTip.
            // Force the ToolTip text to be displayed whether or not the form is active.

            // Set up the ToolTip text for the Button and Checkbox.
            toolTip1.SetToolTip(btnSave, "Saves the selected task.");

            // init notification icon properties
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            //Shows the info icon so the user doesn't think there is an error.
            notifyIcon1.BalloonTipText = "NewApp Configurator is minimized in system tray...";
            notifyIcon1.BalloonTipTitle = "NewApp Service Configurator";
            // this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon"))); //The tray icon to use
            notifyIcon1.Text = "NewApp Service Configurator";


            _logger?.LogDebug("Initialization completed.");
        }

        private void openContainingFolderToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        private void viewLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // For Serilog file logging, construct the log file path based on configuration
                var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                var guiLogFile = Path.Combine(logsDirectory, $"NewAppUI-{DateTime.Now:yyyyMMdd}.log");
                
                var serviceLogFileName = _configuration?["AppSettings:ServiceLogFileNameForLogViewer"] ?? "NewAppService.log";
                var serviceLogFile = Path.Combine(logsDirectory, serviceLogFileName);

                // Use the configured log viewer executable
                var logViewerExe = _configuration?["AppSettings:LogViewerExe"] ?? "baretail.exe";

                if (File.Exists(guiLogFile) || File.Exists(serviceLogFile))
                {
                    // Prepare the process to run
                    var processStartInfo = new ProcessStartInfo
                    {
                        Arguments = serviceLogFile.PutIntoQuotes() + " " + guiLogFile.PutIntoQuotes(),
                        FileName = Path.Combine(Directory.GetCurrentDirectory(), logViewerExe),
                        WindowStyle = ProcessWindowStyle.Normal,
                        CreateNoWindow = false
                    };

                    var process = new Process {StartInfo = processStartInfo};

                    // Runs the log viewer application
                    process.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message + "\n See log file for more informations", "error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger?.LogError(ex, "Error in log viewer operation");
            }
        }

        private void ServiceManager_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500);
                ShowInTaskbar = false;
            }
            else if (FormWindowState.Normal == WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            notifyIcon1.Visible = false;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var box = new AboutBox1();
            box.ShowDialog();
        }
    }
}