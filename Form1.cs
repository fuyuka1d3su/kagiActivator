using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace kagiForms

{
    using SLID = Guid;
    public partial class Form1 : Form
    {
        String edition;
        public Form1()
        {
            InitializeComponent();
            isConnectedToInternet();

            kagiVersionLabel.Text = "Kagi 0.1a";

            EditionText.Text = getEdition().ToString();
            edition = getEdition().ToString();

            bool activated = IsGenuineWindows();
            if (activated)
            {
                ActivationText.Text = "Your OS is already activated";
                ActivationText.ForeColor = System.Drawing.Color.Green;
                activateButton.Enabled = false;
            }
            else
            {
                ActivationText.Text = "Your OS is not currently activated";
                ActivationText.ForeColor = System.Drawing.Color.Red;
                deactivateButton.Enabled = false;
            }
        }

        private void isConnectedToInternet()
        {
            try
            {
                System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();
                System.Net.NetworkInformation.PingReply reply = pingSender.Send("google.com");
            }
            catch
            {
                MessageBox.Show("Kagi couldn't verify you were connected to the Internet. " +
                        "Please connect your device to the Internet before relaunching Kagi.",
                        "No Internet !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
        }

        
        private string getEdition()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");
            return registryKey.GetValue("ProductName").ToString();
        }

        public enum SL_GENUINE_STATE
        {
            SL_GEN_STATE_IS_GENUINE = 0,
            SL_GEN_STATE_INVALID_LICENSE = 1,
            SL_GEN_STATE_TAMPERED = 2,
            SL_GEN_STATE_OFFLINE = 3,
            SL_GEN_STATE_LAST = 4
        }
        
        public enum ERROR_CODES : int
        {
            // IPK (When changing the key)
            NO_ADMIN = -1073418203,
            INVALID_KEY = -1073418160,
            // ATO (When reloading license status)
            LICENSE_MANAGEMENT_ERROR = -1073418124,
            SUCCESS = 0
        }

        [DllImportAttribute("Slwga.dll", EntryPoint = "SLIsGenuineLocal", CharSet = CharSet.None, ExactSpelling = false, SetLastError = false, PreserveSig = true, CallingConvention = CallingConvention.Winapi, BestFitMapping = false, ThrowOnUnmappableChar = false)]
        [PreserveSigAttribute()]
        internal static extern uint SLIsGenuineLocal(ref SLID slid, [In, Out] ref SL_GENUINE_STATE genuineState, IntPtr val3);


        public static bool IsGenuineWindows()
        {
            bool _IsGenuineWindows = false;
            Guid ApplicationID = new Guid("55c92734-d682-4d71-983e-d6ec3f16059f"); //Application ID GUID http://technet.microsoft.com/en-us/library/dd772270.aspx
            SLID windowsSlid = (Guid)ApplicationID;
            try
            {
                SL_GENUINE_STATE genuineState = SL_GENUINE_STATE.SL_GEN_STATE_LAST;
                uint ResultInt = SLIsGenuineLocal(ref windowsSlid, ref genuineState, IntPtr.Zero);
                if (ResultInt == 0)
                {
                    _IsGenuineWindows = (genuineState == SL_GENUINE_STATE.SL_GEN_STATE_IS_GENUINE);
                }
                else
                {
                    Console.WriteLine("Error getting information {0}", ResultInt.ToString());
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return _IsGenuineWindows;
        }

        private String readKeyFromFile()
        {
            try { String text = System.IO.File.ReadAllText(".\\key.txt"); return text; }
            catch { return null; }
        }

        private bool isKeyValid(String key)
        {
            const int CORRECT_KEY_LENGTH = 5;

            if (key == null) { 
                MessageBox.Show("Kagi couldn't find the key.txt file, " +
                    "make sure it is placed in the same directory as the executable", "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error); 
                return false; 
            }

            String[] keysArr = key.Split('-');

            if (keysArr.Length != CORRECT_KEY_LENGTH) {
                MessageBox.Show("The key is not the correct length, " +
                    "make sure the total length of the key is 25.", "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false; 
            }

            foreach (String keyPart in keysArr)
            {
                if (keyPart.Length != CORRECT_KEY_LENGTH) {
                    MessageBox.Show("The 5 parts of the key are invalid. " +
                        "Make sure each part has 5 alphanumeral characters. ", 
                        "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false; 
                }
            }

            MessageBox.Show("Kagi will now try to activate " + edition +
                        " with the key " + key,
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }

        private void activate_click(object sender, EventArgs e)
        {
            String key = readKeyFromFile();

            if (!isKeyValid(key))
            {
                return;
            }

            switch (edition)
            {
                case "Windows 10 Pro":
                    // code to install pro keys
                    activateOS(key);
                    break;

                case "Windows 10 Home":
                    // code to install home keys
                    activateOS(key);
                    break;

                default:
                    MessageBox.Show("Your current OS edition is not currently supported !" +
                "\n\nThe current editions supported by Kagi are Windows Home and Professionnal.",
                "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }
        private bool isError(int code)
        {
            switch (code)
            {
                case (int)ERROR_CODES.NO_ADMIN:
                    MessageBox.Show("The program is missing admin privileges. Please make sure you run Kagi as admin.",
                    "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;

                case (int)ERROR_CODES.INVALID_KEY:
                    MessageBox.Show("There was a problem installing the product key on your system !" +
                    "Please ensure that the product key you entered was made for your Windows Edition and try again.",
                    "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;

                case (int)ERROR_CODES.LICENSE_MANAGEMENT_ERROR:
                    MessageBox.Show("There was a problem installing the product key on your system !" +
                    "This is usually caused by an invalid communication with the activation server. Please try again later.",
                    "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;

                case 0:
                    return false;

                default:
                    MessageBox.Show("An unknown error occured. Please try again later.",
                    "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;

            }
        }

        private void activateOS(String key)
        {
            List<String> commands = new List<String>();

            commands.Add("slmgr.vbs /ipk " + key);
            commands.Add("slmgr.vbs /skms kms.lotro.cc");
            commands.Add("slmgr.vbs /ato");

            int exitCode;

            foreach (String command in commands)
            {
                exitCode = executeCommand(command);
                if (isError(exitCode)) { return; }
            }
            progressBar.PerformStep();

            MessageBox.Show("Your OS was successfully activated !",
                    "Success !", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ActivationText.Text = "Your OS is now activated";
            ActivationText.ForeColor = System.Drawing.Color.Green;
            activateButton.Enabled = false;
            deactivateButton.Enabled = true;


        }

        private int executeCommand(String command)
        {

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + command;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            // get exit code of the process
            int code = process.ExitCode;
            return code;
        }


        private void contributeLink(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/naz-bdeb/kagiActivator",
                UseShellExecute = true
            });
        }

        private void deactivateButton_Click(object sender, EventArgs e)
        {
            executeCommand("slmgr.vbs -upk");
            MessageBox.Show("Your OS was successfully deactivated !",
                    "Success !", MessageBoxButtons.OK, MessageBoxIcon.Information);
            deactivateButton.Enabled = false;
            activateButton.Enabled = true;
        }
    }
}
