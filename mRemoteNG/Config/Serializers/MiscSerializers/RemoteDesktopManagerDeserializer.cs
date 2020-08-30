using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using mRemoteNG.Connection;
using mRemoteNG.Connection.Protocol;
using mRemoteNG.Connection.Protocol.RDP;
using mRemoteNG.Container;
using mRemoteNG.Tree;
using mRemoteNG.Tree.Root;

namespace mRemoteNG.Config.Serializers.MiscSerializers
{
    public class RemoteDesktopManagerDeserializer : IDeserializer<string, ConnectionTreeModel>
    {
        public ConnectionTreeModel Deserialize(string rdcmConnectionsXml)
        {
            var connectionTreeModel = new ConnectionTreeModel();
            var root = new RootNodeInfo(RootNodeType.Connection);

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(rdcmConnectionsXml);

            var rootNode = xmlDocument.SelectSingleNode("/ArrayOfConnection");

            ImportFileOrGroup(rootNode, root);

            connectionTreeModel.AddRootNode(root);
            return connectionTreeModel;
        }

        private static void ImportFileOrGroup(XmlNode xmlNode, ContainerInfo parentContainer)
        {
            var childNodes = xmlNode.SelectNodes("Connection");

            if (childNodes != null)
            {
                // First create containers
                foreach (XmlNode childNode in childNodes)
                {
                    var connectionTypeNode = childNode.SelectSingleNode("ConnectionType")?.InnerText;
                    var name = childNode.SelectSingleNode("Name")?.InnerText;
                    var groupNode = childNode.SelectSingleNode("Group")?.InnerText;

                    switch (connectionTypeNode)
                    {
                        case "Group":
                            var parentGroup = FindParentGroup(TrimEnd(groupNode, name), parentContainer);
                            ImportContainer(childNode, parentGroup ?? parentContainer);
                            break;
                    }
                }

                // Then create connections
                foreach (XmlNode childNode in childNodes)
                {
                    var connectionTypeNode = childNode.SelectSingleNode("ConnectionType")?.InnerText;
                    var groupNode = childNode.SelectSingleNode("Group")?.InnerText;

                    switch (connectionTypeNode)
                    {
                        case "SSHShell":
                        case "RDPConfigured":
                            var conntectionGroup = FindParentGroup(groupNode, parentContainer);
                            ImportServer(childNode, conntectionGroup);
                            break;
                    }
                }
            }
        }

        private static void ImportContainer(XmlNode containerPropertiesNode, ContainerInfo parentContainer)
        {
            var nameNode = containerPropertiesNode.SelectSingleNode("Name");
            var newContainer = new ContainerInfo {Name = nameNode?.InnerText};
            parentContainer.AddChild(newContainer);
        }

        private static void ImportServer(XmlNode serverNode, ContainerInfo parentContainer)
        {
            var newConnectionInfo = ConnectionInfoFromXml(serverNode);
            if (newConnectionInfo != null)
                parentContainer.AddChild(newConnectionInfo);
        }

        private static ConnectionInfo ConnectionInfoFromXml(XmlNode xmlNode)
        {
            switch (xmlNode.SelectSingleNode("ConnectionType")?.InnerText)
            {
                case "RDPConfigured":
                    return GetConnectionInfoRdp(xmlNode);
                case "SSHShell":
                    return GetConnectionInfoSSH(xmlNode);
            }

            return null;
        }

        private static ConnectionInfo GetConnectionInfoSSH(XmlNode xmlNode)
        {
            var connectionInfoShell = new ConnectionInfo
            {
                Protocol = ProtocolType.SSH2,
                Name = xmlNode.SelectSingleNode("Name")?.InnerText,
            };

            var terminalNode = xmlNode.SelectSingleNode("Terminal");
            if (terminalNode != null)
            {
                connectionInfoShell.Hostname = terminalNode.SelectSingleNode("Host")?.InnerText;
                connectionInfoShell.Port = int.Parse(terminalNode.SelectSingleNode("HostPort")?.InnerText ?? "22");
                connectionInfoShell.Username = terminalNode.SelectSingleNode("Username")?.InnerText;
            }

            return connectionInfoShell;
        }

        private static ConnectionInfo GetConnectionInfoRdp(XmlNode xmlNode)
        {
            var url = xmlNode.SelectSingleNode("Url")?.InnerText;
            var urlParts = url?.Split(':');
            var hostname = urlParts?.FirstOrDefault();

            var connectionInfoRDP = new ConnectionInfo
            {
                Protocol = ProtocolType.RDP,
                Name = xmlNode.SelectSingleNode("Name")?.InnerText,
                Hostname = hostname,
                DisplayWallpaper = xmlNode.SelectSingleNode("DisableWallpaper")?.InnerText != "true",
                CacheBitmaps = xmlNode.SelectSingleNode("DisableBitmapCache")?.InnerText != "true",
                DisplayThemes = xmlNode.SelectSingleNode("DisableThemes")?.InnerText != "true",
                RedirectSmartCards = xmlNode.SelectSingleNode("UsesSmartDevices")?.InnerText == "true",
                RedirectDiskDrives = xmlNode.SelectSingleNode("UsesHardDrives")?.InnerText == "true",
                RedirectPrinters = xmlNode.SelectSingleNode("UsesPrinters")?.InnerText == "true",
                RedirectPorts = xmlNode.SelectSingleNode("UsesSerialPorts")?.InnerText != "false",
                UseConsoleSession = xmlNode.SelectSingleNode("Console")?.InnerText == "true",
            };

            var port = urlParts?.Skip(1).Take(1).FirstOrDefault();
            if (int.TryParse(port, out int parsedPort))
                connectionInfoRDP.Port = parsedPort;

            var rdpNode = xmlNode.SelectSingleNode("RDP");

            if (rdpNode != null)
            {
                connectionInfoRDP.Domain = rdpNode.SelectSingleNode("Domain")?.InnerText;
                connectionInfoRDP.Username = rdpNode.SelectSingleNode("UserName")?.InnerText;
                // connectionInfoRDP.Password = DecryptRdManPassword(rdpNode.SelectSingleNode("SafePassword")?.InnerText);
                connectionInfoRDP.RDGatewayHostname = rdpNode.SelectSingleNode("GatewayHostname")?.InnerText;
                connectionInfoRDP.RDGatewayDomain = rdpNode.SelectSingleNode("GatewayDomain")?.InnerText;
                connectionInfoRDP.RDGatewayUsername = rdpNode.SelectSingleNode("GatewayUserName")?.InnerText;
                // connectionInfoRDP.RDGatewayPassword = DecryptRdManPassword(rdpNode.SelectSingleNode("GatewaySafePassword")?.InnerText);

                switch (rdpNode.SelectSingleNode("GatewayUsageMethod")?.InnerText)
                {
                    case "None":
                        connectionInfoRDP.RDGatewayUsageMethod = RDGatewayUsageMethod.Never;
                        break;
                    case "Always":
                        connectionInfoRDP.RDGatewayUsageMethod = RDGatewayUsageMethod.Always;
                        break;
                }
            }

            switch (xmlNode.SelectSingleNode("ScreenColor")?.InnerText)
            {
                case "C256":
                    connectionInfoRDP.Colors = RDPColors.Colors256;
                    break;
                case "C15Bits":
                    connectionInfoRDP.Colors = RDPColors.Colors15Bit;
                    break;
                case "C16Bits":
                    connectionInfoRDP.Colors = RDPColors.Colors16Bit;
                    break;
                case "C24Bits":
                    connectionInfoRDP.Colors = RDPColors.Colors24Bit;
                    break;
                case "C32Bits":
                    connectionInfoRDP.Colors = RDPColors.Colors32Bit;
                    break;
            }

            switch (xmlNode.SelectSingleNode("SoundHook")?.InnerText)
            {
                case "DoNotPlay":
                    connectionInfoRDP.RedirectSound = RDPSounds.DoNotPlay;
                    break;
                case "LeaveAtRemoteComputer":
                    connectionInfoRDP.RedirectSound = RDPSounds.LeaveAtRemoteComputer;
                    break;
            }

            switch (xmlNode.SelectSingleNode("KeyboardHook")?.InnerText)
            {
                default:
                    connectionInfoRDP.RedirectKeys = false;
                    break;
                case "OnTheRemoteComputer":
                    connectionInfoRDP.RedirectKeys = true;
                    break;
            }

            return connectionInfoRDP;
        }

        private static ContainerInfo FindParentGroup(string groupValue, ContainerInfo parentContainer)
        {
            var groupNames = groupValue.Split("\\".ToArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var groupName in groupNames)
            {
                parentContainer = FindGroupWithName(groupName, parentContainer);
            }

            return parentContainer;
        }

        private static ContainerInfo FindGroupWithName(string groupName, ContainerInfo parentContainer)
        {
            return parentContainer.Children.FirstOrDefault(c => c.IsContainer && c.Name == groupName) as ContainerInfo;
        }

        private static string TrimEnd(string input, string suffixToRemove, StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            if (suffixToRemove != null && input.EndsWith(suffixToRemove, comparisonType))
            {
                return input.Substring(0, input.Length - suffixToRemove.Length);
            }

            return input;
        }

        private static string DecryptRdManPassword(string ciphertext)
        {
            if (string.IsNullOrEmpty(ciphertext))
                return string.Empty;

            try
            {
                var plaintextData = ProtectedData.Unprotect(Convert.FromBase64String(ciphertext), new byte[] { }, DataProtectionScope.LocalMachine);
                var charArray = Encoding.Unicode.GetChars(plaintextData);
                return new string(charArray);
            }
            catch (Exception /*ex*/)
            {
                //Runtime.MessageCollector.AddExceptionMessage("RemoteDesktopManager.DecryptPassword() failed.", ex, logOnly: true);
                return string.Empty;
            }
        }
    }
}