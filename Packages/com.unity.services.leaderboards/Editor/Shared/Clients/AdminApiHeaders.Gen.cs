// WARNING: Auto generated code. Modifications will be lost!
// Original source 'com.unity.services.shared' @0.0.11.
using System.Collections.Generic;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Unity.Services.Leaderboards.Clients
{
    class AdminApiHeaders<T>
    {
        readonly string m_GatewayToken;

        public AdminApiHeaders(string gatewayToken)
        {
            m_GatewayToken = gatewayToken;
        }

        public IDictionary<string, string> ToDictionary()
        {
            var packageInfo = ReadPackageInfo();

            return new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {m_GatewayToken}" },
                { "x-client-id", $"{packageInfo.name}@{packageInfo.version}"}
            };
        }

        static PackageInfo ReadPackageInfo()
        {
            return PackageInfo.FindForAssembly(typeof(T).Assembly);
        }
    }
}
