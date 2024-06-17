using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using Patholab_DAL_V1;
using Patholab_DAL_V1.Enums;
using Patholab_XmlService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

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
            this.ntlsCon = ntlsCon;
            this.ntlsSite = ntlsSite;
            this.ntlsUser = ntlsUser;
            this.ServiceProvider = sp;
            Init();
            lblMsg.Content = "";
        }


        private void Init()
        {
            _dal = new DataLayer();
            _dal.Connect(ntlsCon);
        }

        DataLayer _dal;



        List<string> currentSdgList = new List<string>();

        public void AddToLv()
        {

            //Debugger.Launch();
            if (txtPapName.Text == null)
                return;

            if (currentSdgList.Contains(txtPapName.Text))
            {
                lblMsg.Content = "הדרישה רשומה לבקשה";
                txtPapName.Focus();

                return;
            }

            if (!txtPapName.Text.StartsWith("P"))
            {
                lblMsg.Content = "שם הדרישה שגוי או שאינו עומד בתנאים";
                txtPapName.Focus();

                return;
            }

            //var again = _dal.FindBy<U_EXTRA_REQUEST_DATA_USER>(x => x.U_SLIDE_NAME == txtPapName.Text && x.U_REQ_TYPE == "P" && x.U_STATUS == "V").FirstOrDefault();
            //var openPapRequestToSample = dal.FindBy<U_EXTRA_REQUEST_DATA_USER>(x => x.U_SLIDE_NAME == txtPapName.Text && x.U_REQ_TYPE == "P" && x.U_STATUS == "V");

            //if (again != null)
            //{
            //    lblMsg.Content = "קיימת בקשה בתוקף עבור דרישה זו";
            //    txtPapName.Focus();

            //    return;
            //}


            currentSdg = _dal.FindBy<SDG>(x => x.NAME == txtPapName.Text).FirstOrDefault();
            if (currentSdg == null)
            {
                lblMsg.Content = "לא אותרה דרישה";
                txtPapName.Focus();

                return;
            }

            dataGridView1.Items.Add(new DataItem
            {
                U_PATHOLAB_NUMBER = currentSdg.SDG_USER.U_PATHOLAB_NUMBER,
                SDG_NAME = currentSdg.NAME,
                SDG_ID = currentSdg.SDG_ID
            });


            currentSdgList.Add(txtPapName.Text);
            txtPapName.Text = string.Empty;

        }



        private SDG currentSdg;
        private INautilusDBConnection ntlsCon;
        private IExtensionWindowSite2 ntlsSite;
        private INautilusUser ntlsUser;
        private SAMPLE currentSample;

        public INautilusServiceProvider ServiceProvider { get; private set; }

        public void AddPapDilutionRequest()
        {
            try
            {
                lblMsg.Content = "";
                if (dataGridView1.Items.Count == 0)
                {
                    return;
                }

                long currentOperatorId = Convert.ToInt64(ntlsUser.GetOperatorId());
                int cnt = 0;
                foreach (DataItem row in dataGridView1.Items)
                {

                    string U_PATHOLAB_NUMBER = row.U_PATHOLAB_NUMBER;
                    string SDG_NAME = row.SDG_NAME;
                    long SDG_ID = row.SDG_ID;
                    var type = ExtraRequestType.P;
                    currentSample = _dal.FindBy<SDG>(x => x.SDG_ID == SDG_ID).FirstOrDefault().SAMPLEs.FirstOrDefault();

                    //Creating a new slide under the sample
                    FireEventXmlHandler fireEvent = new FireEventXmlHandler(ServiceProvider);
                    fireEvent.CreateFireEventXml("SAMPLE", currentSample.SAMPLE_ID, "Add Emplt Aliquot");
                    bool returnbool = fireEvent.ProcssXml();
                    var new_slide_name = fireEvent.GetValueByTagName("NAME",1);

                    if (returnbool)
                    {
                        cnt++;
                        _dal.Ex_Req_Logic(SDG_ID, new_slide_name.ToString(), type, currentOperatorId,
                              "בקשה לדילול פאפ עבור sample: " + SDG_NAME, "");
                        _dal.InsertToSdgLog(SDG_ID, "PD.ADD_REQ", (long)ntlsCon.GetSessionId(), "בקשה לדילול פאפ");
                        _dal.SaveChanges();

                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show($"בקשה לדילול עבור {currentSample.SDG.NAME} נכשלה");

                    }

                }
                System.Windows.Forms.MessageBox.Show($"בקשות נוספו {cnt} ");
                dataGridView1.Items.Clear();
                currentSdgList.Clear();
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
        public long SDG_ID { get; set; }
    }

}
