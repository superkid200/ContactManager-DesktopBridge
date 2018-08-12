using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Storage;

namespace DataForm
{
    public partial class Form1 : Form
    {
        private const string fileName = @"datastore.xml";
        private string filePath = Path.Combine(
            ApplicationData.Current.LocalFolder.Path,
            "DesktopBridge-Sample",
            fileName);
        private DataSet ds = null;
        public Form1()
        {
            InitializeComponent();
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
    }
}
