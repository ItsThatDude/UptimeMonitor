using System.Security.Cryptography.X509Certificates;

namespace UptimeMonitor.Core.Plugins.PluginBase.Cryptography
{
    public static class X509Certificate2Extensions
    {
        public static bool IsSignedBy(this X509Certificate2 leafCert, X509Certificate2 issuerCert)
        {
            using (var chain = new X509Chain())
            {
                chain.ChainPolicy.ExtraStore.Add(issuerCert);
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

                return chain.Build(leafCert);
            }
        }
    }
}
