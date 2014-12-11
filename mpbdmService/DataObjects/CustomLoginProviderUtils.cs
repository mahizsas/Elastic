using Microsoft.WindowsAzure.Mobile.Service.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace mpbdmService.DataObjects
{
    public class MobileLoginResult 
    {
        public LoginResult loginResult;
        public Users user;
        public MobileLoginResult(Account account, LoginResult loginResult)
        {
            this.user = account.User;
            this.loginResult = loginResult;
        }
    }
    public class LoginRequest
    {
        public String username { get; set; }
        public String email { get; set; }
        public String password { get; set; }
    }
    public class ChangePassRequest
    {
        public String username { get; set; }
        public String password { get; set; }
        public String oldpass { get; set; }
        public String repass { get; set; }
    }

    public class RegistrationRequest
    {
        public String username { get; set; }
        public String password { get; set; }
        public String repass { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }


    }

    public class CustomLoginProviderUtils
    {        public static byte[] hash(string plaintext, byte[] salt)
        {
            SHA512Cng hashFunc = new SHA512Cng();
            byte[] plainBytes = System.Text.Encoding.ASCII.GetBytes(plaintext);
            byte[] toHash = new byte[plainBytes.Length + salt.Length];
            plainBytes.CopyTo(toHash, 0);
            salt.CopyTo(toHash, plainBytes.Length);
            return hashFunc.ComputeHash(toHash);
        }

        public static byte[] generateSalt()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] salt = new byte[256];
            rng.GetBytes(salt);
            return salt;
        }

        public static bool slowEquals(byte[] a, byte[] b)
        {
            int diff = a.Length ^ b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }

    }
}