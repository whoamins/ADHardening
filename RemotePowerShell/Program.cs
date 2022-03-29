using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

namespace MyApp 
{
    internal class Program
    {
        static void Main(string[] args)
        {   
            // IT DOESN'T WORK FOR NOW

            string password = "P@ssw0rd"; // Store it in Properties.Settings.Default
            SecureString pass = new SecureString();

            foreach (Char p in password)
                pass.AppendChar(p);

            PSCredential remoteCredential = new PSCredential("contoso.com\\Administrator", pass);
            WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, "192.168.56.102", 5985, "/wsman",
                "cmd.exe", remoteCredential);
            
            using (Runspace runspace = RunspaceFactory.CreateRunspace(connectionInfo))
            {
                runspace.Open();

                Pipeline pipeline = runspace.CreatePipeline(@"whoami > C:\whoami.txt");

                var results = pipeline.Invoke();
            }
        }
    }
}