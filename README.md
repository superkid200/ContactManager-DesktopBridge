# ContactManager-DesktopBridge
Demostrates how to use App Service in Desktop Bridge

## Run the app
1. Clone the repository using GitHub Desktop, Git Bash, or other Git cloner. If you are unfamiliar with Git or GitHub, you can download the repository as a ZIP file and extract the downloaded ZIP file.
2. Open the Solution file (.sln) file. 
3. If you don't use Visual Studio 2017 with Windows 10 April Update, you will requested to retarget the solution. Follow the instructions in the Retarget Solution window.
4. Choose not to deploy the UWPHead project in the Configuration Manager.
5. Choose Release x86 or x64 as the Configuration.
6. Build and deploy the solution.

NOTE: If you want to open the sln file correctly, you need Visual Studio 2017 version 15.5 with Universal Windows Platform workload configured.
## Build it yourself
### Build the host app
1. Create an Windows Forms application and name the project DataForm.
2. Add a XML file called datastore.xml and enter this data:

`<?xml version="1.0" standalone="yes"?>
<contacts>
  <contact>
    <ID>133-23-7893</ID>
    <Name>Patrick Hines</Name>
    <Phone>206-555-0144</Phone>
    <City>Mercer Island</City>
    <State>WA</State>
  </contact>
  <contact>
    <ID>233-63-4491</ID>
    <Name>Johnson White</Name>
    <Phone>510-555-0144</Phone>
    <City>Oakland</City>
    <State>CA</State>
  </contact>
</contacts>`

3. Open Form1.cs in Design view and add these controls and properties

| Control        | Property           | Value  |
| ------------- |:-------------:| -----:|
| DataGridView    | Anchor | All |
| DataGridView    |Name|dgView
| Button      | Name      |   btnSave |

4. Add this code to Form1.cs in Code view inside the Form1 class

        private const string fileName = @"datastore.xml";
        private string filePath = Path.Combine(
            ApplicationData.Current.LocalFolder.Path,
            "DesktopBridge-Sample",
            fileName);
        private DataSet ds = null;
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load
            btnSave.Click += btnSave_Click
            FormClosing += Form_Closing;
        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            ds.WriteXml(filePath);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(!File.Exists(filePath))
            {
                if(!Directory.Exists(Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    "DesktopBridge-Sample")))
                {
                    Directory.CreateDirectory(Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    "DesktopBridge-Sample"));
                }
                string installDirectory = Assembly.GetExecutingAssembly().Location;
                DirectoryInfo info = Directory.GetParent(installDirectory);
                File.Copy(Path.Combine(info.FullName, fileName), filePath);
            }
            ds = new DataSet();
            ds.ReadXml(filePath);
            dgView.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
            dgView.AutoGenerateColumns = true;
            dgView.DataSource = ds;
            dgView.DataMember = "contact";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ds.WriteXml(filePath);
        }
 4. Create a new UWP Project called UWPHead
 5. Create a new Windows Runtime Component called BackgroundTasks
 6. Rename Class1.cs  to ContactAppService.cs
 7. Replace the entire ContactAppService.cs file content to this line of code:

        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using System.Threading.Tasks;
        using Windows.ApplicationModel.AppService;
        using Windows.ApplicationModel.Background;
        using Windows.Data.Xml.Dom;
        using Windows.Foundation.Collections;
        using Windows.Storage;

        namespace BackgroundTasks
        {
            public sealed class ContactAppService : IBackgroundTask
            {
                AppServiceConnection connection;
                private BackgroundTaskDeferral deferral;
                public void Run(IBackgroundTaskInstance taskInstance)
                {
                    deferral = taskInstance.GetDeferral();
                    taskInstance.Canceled += TaskInstance_Canceled;
                    var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;
                    connection = triggerDetails.AppServiceConnection;
                    connection.RequestReceived += Connection_RequestReceived;
                }

                private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
                {
                    if(deferral != null)
                    {
                        deferral.Complete();
                    }
                }

                private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
                {
                    var messageDeferral = args.GetDeferral();
                    var message = args.Request.Message;
                    string id = message["ID"].ToString();
                    StorageFile file =
                        await (await ApplicationData.Current.LocalFolder.GetFolderAsync("DesktopBridge-Sample")).
                        GetFileAsync("datastore.xml");
                    string xml = await FileIO.ReadTextAsync(file);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    var node = doc.SelectSingleNode("descendant::contact[ID = '" + id + "']");
                    ValueSet response = new ValueSet();
                    if (node != null)
                    {
                        response.Add("Name",
                            node.SelectSingleNode("descendant::Name").InnerText);
                        response.Add("Phone",
                            node.SelectSingleNode("descendant::Phone").InnerText);
                        response.Add("City",
                            node.SelectSingleNode("descendant::City").InnerText);
                        response.Add("State",
                            node.SelectSingleNode("descendant::State").InnerText);
                    }
                    else
                    {
                        response.Add("Name",
                            "NO RECORDS FOUND");
                        response.Add("Phone",
                            "NO RECORDS FOUND");
                        response.Add("City",
                            "NO RECORDS FOUND");
                        response.Add("State",
                            "NO RECORDS FOUND");
                    }
                    AppServiceResponseStatus status = await
                        args.Request.SendResponseAsync(response);
                    messageDeferral.Complete();
                }
            }
        }

8.  Add a reference of BackgroundTasks project to UWPHead project.
9. Create a Windows Packaging Project.
10. Open Package.appxmanifest in code view and add this line of code in the Application tag:

         <Extensions>
          <uap:Extension Category="windows.appService" EntryPoint="BackgroundTasks.ContactAppService">
            <uap:AppService Name="ContactAppService" />
          </uap:Extension>
        </Extensions>
        
11. Choose Release x86 or x64, and choose NOT to deploy UWPHead in the Configuration Manager.
12. Build and deploy the application. Note the Package Family Name (PFN) because we will need it in the client app.
### Build the client app
1. Create a new UWP project.
2. Open MainPage.xaml and copy this code inside the Page tag:

        <Grid Background="MintCream">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="1*"/>
            </Grid.RowDefinitions>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="1*"/>
            </Grid.ColumnDefinitions>
            <TextBlock Margin="12" VerticalAlignment="Center" Text="App Service Connection:"/>
            <TextBlock Margin="12" VerticalAlignment="Center" Grid.Row="1" Text="Request:"/>
            <TextBlock Margin="12" VerticalAlignment="Top" Grid.Row="2" Text="Response:"/>
            <TextBlock Name="tbStatus" Grid.Column="1" Margin="12" VerticalAlignment="Center" Text="Status: Closed" Foreground="Red"/>
            <TextBox Grid.Row="1" Name="tbId" Grid.Column="1" Margin="12" Width="200" Text="133-23-7893"/>
            <Button Name="btnConnect" Grid.Column="2" Margin="12" Content="Connect" Click="btnConnect_Click"/>
            <Button Name="btnQuery" Grid.Column="2" Margin="12" Grid.Row="1" Content="Submit" IsEnabled="False" Click="btnQuery_Click"/>
            <StackPanel Margin="12" Name="responseStack" Grid.Row="2" Grid.Column="1">
                <TextBlock Name="tbName" Foreground="Green"/>
                <TextBlock Name="tbPhone" Foreground="Green"/>
                <TextBlock Name="tbCity" Foreground="Green"/>
                <TextBlock Name="tbState" Foreground="Green"/>
            </StackPanel>
        </Grid>
3. Copy this code in MainPage.xaml.cs

        AppServiceConnection connection = null;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if(connection == null)
            {
                btnConnect.IsEnabled = false;
                connection = new AppServiceConnection();
                connection.AppServiceName = "ContactAppService";
                connection.PackageFamilyName = "<Replace with the Package Family Name of the host app>";
                var status = await connection.OpenAsync();
                btnConnect.IsEnabled = true;
                tbStatus.Text = "Status: " + status.ToString();
                if(status == AppServiceConnectionStatus.Success)
                {
                    tbStatus.Foreground = new SolidColorBrush(Colors.Green);
                    btnConnect.Content = "Disconnect";
                    btnQuery.IsEnabled = true;
                }
                else
                {
                    btnConnect.Content = "Cleanup";
                }
            }
            else
            {
                connection = null;
                tbStatus.Text = "Status: Closed";
                tbStatus.Foreground = new SolidColorBrush(Colors.Red);
                btnConnect.Content = "Connect";
                btnQuery.IsEnabled = false;
                tbName.Text = "";
                tbPhone.Text = "";
                tbState.Text = "";
                tbCity.Text = "";
            }
        }

        private async void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            ValueSet request = new ValueSet();
            request.Add("ID", tbId.Text);
            AppServiceResponse response = await connection.SendMessageAsync(request);
            if(response.Status == AppServiceResponseStatus.Success)
            {
                tbName.Text = "Name: " + response.Message["Name"].ToString();
                tbPhone.Text = "Phone: " + response.Message["Phone"].ToString();
                tbCity.Text = "City: " + response.Message["City"].ToString();
                tbState.Text = "State: " + response.Message["State"].ToString();
            }
        }
 4.Build and deploy it and test.
