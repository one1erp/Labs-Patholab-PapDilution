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

            try
            {
                var w = new UserControl1(sp, xmlProcessor, _ntlsCon, _ntlsSite, _ntlsUser);
                elementHost1.Child = w;
                w.Focus();

            }
            catch (Exception e)
            {


                MessageBox.Show("Error in  InitializeData " + "/n" + e.Message, mboxHeader);
                Logger.WriteLogFile(e);
            }

        }
        #endregion


    }

   
}



