using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace FtpConnector
{
    static class Program
    {
        /// <summary>
        /// Structure required by the Windows API to handle credential information.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct CREDENTIAL
        {
            public uint Flags;
            public uint Type;
            public string TargetName;
            public string Comment;
            public long LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
        }

        [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CredRead(string target, uint type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
        static extern void CredFree(IntPtr buffer);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string ftpServer = "kt.insidden.com";
            
            // The exact target name format that Windows Explorer expects for FTP credentials
            string targetName = String.Format("ftp://{0}", ftpServer); 
            string connectionUrl = ""; 

            string savedUser = "";
            string savedPass = "";
            bool hasSavedCredentials = false;

            // Attempt to read saved credentials via Windows API (advapi32.dll)
            IntPtr credPtr;
            if (CredRead(targetName, 1 /* CRED_TYPE_GENERIC */, 0, out credPtr))
            {
                CREDENTIAL cred = (CREDENTIAL)Marshal.PtrToStructure(credPtr, typeof(CREDENTIAL));
                savedUser = cred.UserName;
                
                if (cred.CredentialBlob != IntPtr.Zero && cred.CredentialBlobSize > 0)
                {
                    savedPass = Marshal.PtrToStringUni(cred.CredentialBlob, (int)cred.CredentialBlobSize / 2);
                }
                
                CredFree(credPtr); 
                hasSavedCredentials = true;
            }

            // Display the login form, passing targetName so the Clear button can access it
            using (LoginForm loginForm = new LoginForm(savedUser, savedPass, hasSavedCredentials, targetName))
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    string username = loginForm.Username;
                    string password = loginForm.Password;
                    
                    // Always URL-encode credentials to safely handle special characters
                    string encodedUser = Uri.EscapeDataString(username);
                    string encodedPass = Uri.EscapeDataString(password);
                    
                    // ALWAYS pass both username and password directly to Explorer via URL.
                    // This completely bypasses Explorer's buggy internal credential resolution.
                    connectionUrl = String.Format("ftp://{0}:{1}@{2}/", encodedUser, encodedPass, ftpServer);

                    if (loginForm.RememberMe)
                    {
                        // Save to Credential Manager ONLY for our C# app to read on next launch
                        SaveCredentials(targetName, username, password);
                    }
                    else
                    {
                        if (hasSavedCredentials)
                        {
                            DeleteCredentials(targetName);
                        }
                    }
                }
                else
                {
                    return; // Exit application if the user clicks 'Cancel'
                }
            }

            // Launch Windows Explorer explicitly asking the shell to handle the foreground focus
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = connectionUrl,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal
            };
            
            Process.Start(startInfo);

            // Keep the application alive for a brief moment in the background (1 second).
            // This prevents the parent window (like cmd.exe or the previous folder) 
            // from stealing focus back before the new Explorer window has fully initialized.
            Thread.Sleep(1000);
        }

        internal static void SaveCredentials(string targetName, string username, string password)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmdkey.exe",
                Arguments = String.Format("/generic:{0} /user:{1} /pass:{2}", targetName, username, password),
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo)) { process.WaitForExit(); }
        }

        internal static void DeleteCredentials(string targetName)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmdkey.exe",
                Arguments = String.Format("/delete:{0}", targetName),
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo)) { process.WaitForExit(); }
        }
    }

    /// <summary>
    /// A custom form class representing the FTP authentication UI window.
    /// </summary>
    public class LoginForm : Form
    {
        public string Username { get { return txtUser.Text; } }
        public string Password { get { return txtPass.Text; } }
        public bool RememberMe { get { return chkRemember.Checked; } }

        private TextBox txtUser;
        private TextBox txtPass;
        private CheckBox chkRemember;

        public LoginForm(string initialUser, string initialPass, bool checkRemember, string targetName)
        {
            this.Text = "FTP Authentication";
            this.Size = new Size(340, 265); 
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblUser = new Label { Text = "Username:", Location = new Point(20, 20), Size = new Size(100, 20) };
            txtUser = new TextBox { Location = new Point(120, 18), Size = new Size(180, 20), Text = initialUser };

            Label lblPass = new Label { Text = "Password:", Location = new Point(20, 60), Size = new Size(100, 20) };
            txtPass = new TextBox { Location = new Point(120, 58), Size = new Size(180, 20), UseSystemPasswordChar = true, Text = initialPass };

            chkRemember = new CheckBox { Text = "Save credentials", Location = new Point(120, 95), Size = new Size(180, 20), Checked = checkRemember };

            Button btnOk = new Button { Text = "Connect", Location = new Point(120, 135), Size = new Size(85, 28), DialogResult = DialogResult.OK };
            Button btnCancel = new Button { Text = "Cancel", Location = new Point(215, 135), Size = new Size(85, 28), DialogResult = DialogResult.Cancel };

            Button btnClear = new Button { Text = "Clear Saved Data", Location = new Point(120, 175), Size = new Size(180, 28) };
            btnClear.Click += (sender, e) => 
            {
                Program.DeleteCredentials(targetName);
                txtUser.Text = "";
                txtPass.Text = "";
                chkRemember.Checked = false;
                MessageBox.Show("Saved credentials have been removed from the system.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            this.Controls.Add(lblUser);
            this.Controls.Add(txtUser);
            this.Controls.Add(lblPass);
            this.Controls.Add(txtPass);
            this.Controls.Add(chkRemember);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
            this.Controls.Add(btnClear);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}