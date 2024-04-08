using Patholab_Common;
using Patholab_DAL_V1.Enums;
using Patholab_DAL_V1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ADODB;
using LSSERVICEPROVIDERLib;
using LSExtensionWindowLib;
using System.Reflection;

namespace PapDilution
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
     

        public UserControl1(INautilusServiceProvider sp, INautilusProcessXML xmlProcessor, INautilusDBConnection ntlsCon, IExtensionWindowSite2 ntlsSite, INautilusUser ntlsUser)
        {
            InitializeComponent();
            this.sp = sp;
            this.xmlProcessor = xmlProcessor;
            this.ntlsCon = ntlsCon;
            this.ntlsSite = ntlsSite;
            this.ntlsUser = ntlsUser;
            Init();
            lblMsg.Content = "";
        }

       
        private void Init()
        {
           _dal = new DataLayer();
            _dal.Connect(ntlsCon);
        }

        DataLayer _dal;



        List<string> currentSamplesList = new List<string>();

        private void AddToLv()
        {

            //Debugger.Launch();
            if (txtPapName.Text == null)
                return;

            if (currentSamplesList.Contains(txtPapName.Text))
            {
                lblMsg.Content = "הצנצנת רשומה לבקשה";
                txtPapName.Focus();

                return;
            }

            if (!txtPapName.Text.StartsWith("P"))
            {
                lblMsg.Content = "שם הצנצנת שגוי או שאינו עומד בתנאים";
                txtPapName.Focus();

                return;
            }

            var again = _dal.FindBy<U_EXTRA_REQUEST_DATA_USER>(x => x.U_SLIDE_NAME == txtPapName.Text && x.U_REQ_TYPE == "P" && x.U_STATUS == "V").FirstOrDefault();

            //var openPapRequestToSample = dal.FindBy<U_EXTRA_REQUEST_DATA_USER>(x => x.U_SLIDE_NAME == txtPapName.Text && x.U_REQ_TYPE == "P" && x.U_STATUS == "V");

            if (again != null)
            {
                lblMsg.Content= "קיימת בקשה בתוקף עבור צנצנת זו";
                txtPapName.Focus();

                return;
            }


            currentSample = _dal.FindBy<SAMPLE>(x => x.NAME == txtPapName.Text).FirstOrDefault();
            if (currentSample == null)
            {
                lblMsg.Content = "לא אותרה צנצנת";
                txtPapName.Focus();

                return;
            }

            currentSdg = currentSample.SDG;

            dataGridView1.Items.Add(new DataItem
            {
                U_PATHOLAB_NUMBER = currentSdg.SDG_USER.U_PATHOLAB_NUMBER,
                SDG_NAME = currentSdg.NAME,
                SAMPLE_NAME = currentSample.NAME,
                SDG_ID = currentSdg.SDG_ID
            });

            
            currentSamplesList.Add(txtPapName.Text);
            txtPapName.Text = string.Empty;



        }



        private SDG currentSdg;
        private SAMPLE currentSample;
        private INautilusServiceProvider sp;
        private INautilusProcessXML xmlProcessor;
        private INautilusDBConnection ntlsCon;
        private IExtensionWindowSite2 ntlsSite;
        private INautilusUser ntlsUser;

        private void AddPapDilutionRequest()
        {
            try
            {
                lblMsg.Content = "";
                if (dataGridView1.Items.Count == 0)
                {
                    return;
                }

                long currentOperatorId = Convert.ToInt64(ntlsUser.GetOperatorId());
                foreach (DataItem row in dataGridView1.Items)
                {

                    string U_PATHOLAB_NUMBER = row.U_PATHOLAB_NUMBER;
                    string SDG_NAME = row.SDG_NAME;
                    string SAMPLE_NAME = row.SAMPLE_NAME;
                    long SDG_ID = row.SDG_ID;
                    var type = ExtraRequestType.P;

                    _dal.Ex_Req_Logic(SDG_ID, SAMPLE_NAME, type, currentOperatorId,
                        "בקשה לדילול פאפ עבור sample: " + currentSample.NAME, "");
                    _dal.InsertToSdgLog(SDG_ID, "PD.ADD_REQ", (long)ntlsCon.GetSessionId(), "בקשה לדילול פאפ");
                    _dal.SaveChanges();
                }

                System.Windows.Forms.MessageBox.Show("בקשות לדילול פאפ נוצרו בהצלחה");
                dataGridView1.Items.Clear();
                currentSamplesList.Clear();
                txtPapName.Focus();



            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("בקשה נכשלה");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddPapDilutionRequest();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dg = System.Windows.MessageBox.Show("האם אתה בטוח שברצונך לצאת?", "", MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            if (dg == MessageBoxResult.Yes)
            {
                ntlsSite.CloseWindow();
            }
        }

        private void txtPapName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                lblMsg.Content = "";
                AddToLv();
            }
        }

        private void imageContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dg = System.Windows.MessageBox.Show("האם אתה בטוח שברצונך לצאת?", "", MessageBoxButton.YesNoCancel,
               MessageBoxImage.Question);
            if (dg == MessageBoxResult.Yes)
            {
                ntlsSite.CloseWindow();
            }
        }

        private void authorizeBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddPapDilutionRequest();
        }
    }
    public class DataItem
    {
        public string U_PATHOLAB_NUMBER { get; set; }
        public string SDG_NAME { get; set; }
        public string SAMPLE_NAME { get; set; }
        public long SDG_ID { get; set; }
    }

}
