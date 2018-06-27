
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

// This class method's are used to convert a pem file into an X509Certificate2 class
public class Util
{
    public static X509Certificate2 ConvertFromPfxToPem(string filename)
    {
        using (System.IO.FileStream fs = System.IO.File.OpenRead(filename))
        {
            byte[] data = new byte[fs.Length];
            byte[] res = null;
            fs.Read(data, 0, data.Length);
            if (data[0] != 0x30)
            {
                res = GetPem("CERTIFICATE", data);
            }
            X509Certificate2 x509 = new X509Certificate2(res); //Exception hit here
            return x509;
        }      
    }   

    private static byte[] GetPem(string type, byte[] data)
    {
        string pem = Encoding.UTF8.GetString(data);
        string header = String.Format("-----BEGIN {0}-----", type);
        string footer = String.Format("-----END {0}-----", type);
        int start = pem.IndexOf(header) + header.Length;
        int end = pem.IndexOf(footer, start);
        string base64 = pem.Substring(start, (end - start));
        base64 = base64.Replace(System.Environment.NewLine, "");
        base64 = base64.Replace('-', '+');
        base64 = base64.Replace('_', '/');
        return Convert.FromBase64String(base64);
    }

    public static RSACryptoServiceProvider PemFileReader(){
        RsaPrivateCrtKeyParameters keyParams;
        using (var reader = File.OpenText("cert.pem")) // file containing RSA PKCS1 private key
        {
            keyParams = ((RsaPrivateCrtKeyParameters)new PemReader(reader).ReadObject());
        }

        RSAParameters rsaParameters = new RSAParameters();
        rsaParameters.Modulus = keyParams.Modulus.ToByteArrayUnsigned();
        rsaParameters.P = keyParams.P.ToByteArrayUnsigned();
        rsaParameters.Q = keyParams.Q.ToByteArrayUnsigned();
        rsaParameters.DP = keyParams.DP.ToByteArrayUnsigned();
        rsaParameters.DQ = keyParams.DQ.ToByteArrayUnsigned();
        rsaParameters.InverseQ = keyParams.QInv.ToByteArrayUnsigned();
        rsaParameters.D = keyParams.Exponent.ToByteArrayUnsigned();
        rsaParameters.Exponent = keyParams.PublicExponent.ToByteArrayUnsigned();
        RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider(2048);
        rsaKey.ImportParameters(rsaParameters);
        return rsaKey;
    }
}