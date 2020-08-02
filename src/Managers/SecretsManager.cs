// https://github.com/ilyalozovyy/credentialmanagement

using CredentialManagement;

namespace Regata.Measurements
{
    public class UserCredential
    {
        public UserCredential(string name, string secret)
        {
            this.Name = name;
            this.Secret = secret;
        }
        public string Name { get; set; }
        public string Secret { get; set; }

    }

    public static class SecretsManager
    {
        public static UserCredential GetCredential(string target)
        {
            var cm = new Credential { Target = target };
            if (!cm.Load())
            {
                return null;
            }

            return new UserCredential(cm.Username, cm.Password);
        }

        public static bool SetCredential(
             string target, string username, string password)
        {
            return new Credential
            {
                Target = target,
                Username = username,
                Password = password,
                PersistanceType = PersistanceType.LocalComputer
            }.Save();
        }

        public static bool RemoveCredentials(string target)
        {
            return new Credential { Target = target }.Delete();
        }
    }
}