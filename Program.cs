using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace dotnetconsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"This Application must be run after running the powershell script Setup.ps1!
                                This DotNet Console Application authenticates to Key Vault!
                                It also creates a Secret Key Value Pair!
                                And then it gets the Secret Key Value Pair!");
            
            bool isWindows = System.Runtime.InteropServices.RuntimeInformation
                                               .IsOSPlatform(OSPlatform.Windows);
            string KEYVAULT_URI = String.Empty;
            string APPLICATION_ID = String.Empty;
            string CERT_THUMBPRINT = String.Empty;
            if(isWindows)
            {
                KEYVAULT_URI = System.Environment.GetEnvironmentVariable("VAULT_NAME", EnvironmentVariableTarget.User);
                APPLICATION_ID = System.Environment.GetEnvironmentVariable("APPLICATION_ID", EnvironmentVariableTarget.User);
                CERT_THUMBPRINT = System.Environment.GetEnvironmentVariable("CERT_THUMBPRINT", EnvironmentVariableTarget.User);    
            }
            else 
            {
                var result = GetVariablesFromLocalFiles();
                APPLICATION_ID = result.Item1;
                CERT_THUMBPRINT = result.Item2;
                KEYVAULT_URI = result.Item3;
            }            
            KeyVault keyVaultObj = new KeyVault(APPLICATION_ID, CERT_THUMBPRINT);
            var VaultName = "https://" + KEYVAULT_URI + ".vault.azure.net/";            
            var waitHandle = keyVaultObj.CreateSecretKeyValuePair(VaultName);    
            Console.WriteLine("Vault URI is! {0}", VaultName);
            Console.WriteLine("Wait method is invoked to wait for Secret Key Value pair to be created");
            waitHandle.Wait();
            Console.WriteLine("Secret Key Value pair is now created");            
            keyVaultObj.GetResult(VaultName);
        }

        /*
            Helper method used to parse local files created. 
            This is helpful when running this sample on Mac/Linux
        */
        private static Tuple<string, string, string> GetVariablesFromLocalFiles()
        {
            var ServicePrincipalJSON = Directory.GetCurrentDirectory() + "/ServicePrincipal.json";
            var CertThumbprintJSON = Directory.GetCurrentDirectory() + "/CertThumbprint.txt";
            var VaultJSON = Directory.GetCurrentDirectory() + "/KeyVault.json";
            if(File.Exists(ServicePrincipalJSON) && File.Exists(CertThumbprintJSON) && File.Exists(VaultJSON))
            {
                return new Tuple<string, string, string>(ProcessFile(ServicePrincipalJSON, "appId", true), ProcessFile(CertThumbprintJSON, "", false), ProcessFile(VaultJSON, "name", true));
            } 

            return new Tuple<string, string, string>("", "", "");
        }

        /*
            Helper method used to parse local files created. 
            This is helpful when running this sample on Mac/Linux
        */
        private static string ProcessFile(string fileName, string valueToLookFor, bool isJson)
        {
            var result = "";
            using (StreamReader ContentsOfFile = File.OpenText(fileName))
            {
                if(isJson){
                    var stuff = (JObject)JsonConvert.DeserializeObject(ContentsOfFile.ReadToEnd());
                    result = stuff[valueToLookFor].Value<string>();
                }
                else {
                    var contents = ContentsOfFile.ReadToEnd();
                    contents = contents.Split("=")[1];
                    result = Regex.Replace(contents, @"\t|\n|\r", "");
                }
            }            
            return result;
        }
    }
}