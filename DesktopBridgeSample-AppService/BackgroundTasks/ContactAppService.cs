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
