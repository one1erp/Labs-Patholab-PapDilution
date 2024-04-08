using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using Patholab_Common;
using Patholab_DAL_V1;
using Patholab_XmlService;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Oracle.DataAccess.Client;
using System.Configuration;
using System.Diagnostics;
using UserControl = System.Windows.Forms.UserControl;
using System.Timers;
using Patholab_DAL_V1.Enums;
using System.Windows;
using MessageBox = System.Windows.MessageBox;


namespace PapDilution
{


    [ComVisible(true)]
    [ProgId("PapDilution.PapDilutionCls")]
    public partial class PapDilutionCls : UserControl, IExtensionWindow
    {

        #region Private fields

        private INautilusProcessXML xmlProcessor;
        private IExtensionWindowSite2 _ntlsSite;
        private INautilusServiceProvider sp;
        private INautilusDBConnection _ntlsCon;
        private INautilusUser _ntlsUser;

        private DataLayer dal;
        public bool DEBUG;
        private string mboxHeader = "מסך אימות דגימה -  Nautilus";


        #endregion

        #region Ctor

        public PapDilutionCls()
        {
            
            InitializeComponent();                
            lblMsg.Text = "";

        }

      

        #endregion



        #region Implementation of IExtensionWindow

        public bool CloseQuery()
        {
            if (dal != null) dal.Close();
            this.Dispose();
            return true;
        }

        public void Internationalise()
        {
        }

        public void SetSite(object site)
        {
            _ntlsSite = (IExtensionWindowSite2)site;

            _ntlsSite.SetWindowInternalName("PapDilution");
            _ntlsSite.SetWindowRegistryName("PapDilution");
            _ntlsSite.SetWindowTitle("דילול פאפ");
        }




        public void PreDisplay()
        {

            xmlProcessor = Utils.GetXmlProcessor(sp);

            Utils.GetNautilusUser(sp);

            InitializeData();

        }

        public WindowButtonsType GetButtons()
        {
            return LSExtensionWindowLib.WindowButtonsType.windowButtonsNone;
        }

        public bool SaveData()
        {
            return false;
        }

        public void SetServiceProvider(object serviceProvider)
        {
            sp = serviceProvider as NautilusServiceProvider;
            _ntlsCon = Utils.GetNtlsCon(sp);
            _ntlsUser = Utils.GetNautilusUser(sp);

        }

        public void SetParameters(string parameters)
        {

        }

        public void Setup()
        {

        }

        public WindowRefreshType DataChange()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public WindowRefreshType ViewRefresh()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public void refresh()
        {
        }

        public void SaveSettings(int hKey)
        {
        }

        public void RestoreSettings(int hKey)
        {
        }

        public void Close()
        {

        }

        #endregion


        #region Initilaize

        public void InitializeData()
        {


            //    Debugger.Launch();
            try
            {
                var w = new UserControl1(sp, xmlProcessor, _ntlsCon, _ntlsSite, _ntlsUser);
                elementHost1.Child = w;
                //w.Initilaize();
                w.Focus();





            }
            catch (Exception e)
            {


                MessageBox.Show("Error in  InitializeData " + "/n" + e.Message, mboxHeader);
                Logger.WriteLogFile(e);
            }

        }






        #endregion




        private void textBox1_KeDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                lblMsg.Text = "";
                AddToLv();

            }

        }

        List<string> currentSamplesList = new List<string>();

        private void AddToLv()
        {

            //Debugger.Launch();
            if (txtPapName.Text == null)
                return;

            if (currentSamplesList.Contains(txtPapName.Text))
            {
                lblMsg.Text = "הצנצנת רשומה לבקשה";
                return;
            }

            if (!txtPapName.Text.StartsWith("P"))
            {
                lblMsg.Text = "שם הצנצנת שגוי או שאינו עומד בתנאים";
                return;
            }

            var again = dal.FindBy<U_EXTRA_REQUEST_DATA_USER>(x => x.U_SLIDE_NAME == txtPapName.Text && x.U_REQ_TYPE == "P" && x.U_STATUS == "V").FirstOrDefault();

            //var openPapRequestToSample = dal.FindBy<U_EXTRA_REQUEST_DATA_USER>(x => x.U_SLIDE_NAME == txtPapName.Text && x.U_REQ_TYPE == "P" && x.U_STATUS == "V");

            if (again != null)
            {
                lblMsg.Text = "קיימת בקשה בתוקף עבור צנצנת זו";
                return;
            }


            currentSample = dal.FindBy<SAMPLE>(x => x.NAME == txtPapName.Text).FirstOrDefault();
            if (currentSample == null)
            {
                lblMsg.Text = "לא אותרה צנצנת";
                return;
            }

            currentSdg = currentSample.SDG;

            dataGridView1.Rows.Add(currentSdg.SDG_USER.U_PATHOLAB_NUMBER, currentSdg.NAME, currentSample.NAME,
                currentSdg.SDG_ID);
            currentSamplesList.Add(txtPapName.Text);
            txtPapName.Text = "";


        }



        private SDG currentSdg;
        private SAMPLE currentSample;

        private void button1_Click(object sender, EventArgs e)
        {
            AddPapDilutionRequest();
        }

        private void AddPapDilutionRequest()
        {
            try
            {
                lblMsg.Text = "";
                if (dataGridView1.Rows.Count == 0)
                {
                    return;
                }

                long currentOperatorId = Convert.ToInt64(_ntlsUser.GetOperatorId());
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    DataGridViewCellCollection cells = row.Cells;

                    string U_PATHOLAB_NUMBER = cells[0].Value.ToString();
                    string SDG_NAME = cells[1].Value.ToString();
                    string SAMPLE_NAME = cells[2].Value.ToString();
                    long SDG_ID = Convert.ToInt64(cells[3].Value);
                    var type = ExtraRequestType.P;

                    dal.Ex_Req_Logic(SDG_ID, SAMPLE_NAME, type, currentOperatorId,
                        "בקשה לדילול פאפ עבור sample: " + currentSample.NAME, "");
                    dal.InsertToSdgLog(SDG_ID, "PD.ADD_REQ", (long)_ntlsCon.GetSessionId(), "בקשה לדילול פאפ");
                    dal.SaveChanges();
                }

                System.Windows.Forms.MessageBox.Show("בקשות לדילול פאפ נוצרו בהצלחה");
                dataGridView1.Rows.Clear();



            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("בקשה נכשלה");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var dg = MessageBox.Show("האם אתה בטוח שברצונך לצאת?","", MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            if (dg == MessageBoxResult.Yes)
            {
                _ntlsSite.CloseWindow();
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void txtPapName_TextChanged(object sender, EventArgs e)
        {

        }

        private void lblMsg_Click(object sender, EventArgs e)
        {

        }
    }

   
}



