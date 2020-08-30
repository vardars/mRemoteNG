using System.Linq;
using mRemoteNG.Config.Serializers.MiscSerializers;
using mRemoteNG.Connection;
using mRemoteNG.Connection.Protocol;
using mRemoteNG.Connection.Protocol.RDP;
using mRemoteNG.Container;
using mRemoteNG.Tree;
using mRemoteNGTests.Properties;
using NUnit.Framework;

namespace mRemoteNGTests.Config.Serializers.MiscSerializers
{
    public class RemoteDesktopManagerDeserializerTests
    {
        private string _connectionFileContents;
        private RemoteDesktopManagerDeserializer _deserializer;
        private ConnectionTreeModel _connectionTreeModel;
        
        private const string ExpectedGroupName1 = "Group1";
        private const string ExpectedGroupName2 = "Group2";
        
        private const string ExpectedName = "server1_displayname";
        private const string ExpectedHostname = "server1";
        private const string ExpectedUsername = "myusername1";
        private const string ExpectedDomain = "mydomain";
        // private const string ExpectedPassword = "passwordHere!";
        private const bool ExpectedUseConsoleSession = true;
        private const int ExpectedPort = 9933;
        private const RDGatewayUsageMethod ExpectedGatewayUsageMethod = RDGatewayUsageMethod.Always;
        private const string ExpectedGatewayHostname = "gatewayserverhost.innerdomain.net";
        private const string ExpectedGatewayUsername = "gatewayusername";
        private const string ExpectedGatewayDomain = "innerdomain";
        // private const string ExpectedGatewayPassword = "gatewayPassword123";
        private const RDPResolutions ExpectedRdpResolution = RDPResolutions.FitToWindow;
        private const RDPColors ExpectedRdpColorDepth = RDPColors.Colors24Bit;
        private const RDPSounds ExpectedAudioRedirection = RDPSounds.DoNotPlay;
        private const bool ExpectedKeyRedirection = true;
        private const bool ExpectedSmartcardRedirection = true;
        private const bool ExpectedDriveRedirection = true;
        private const bool ExpectedPortRedirection = true;
        private const bool ExpectedPrinterRedirection = true;
        
        private const string ExpectedName2 = "server2_displayname";
        private const string ExpectedHostname2 = "server2";
        private const string ExpectedUsername2 = "myusername2";
        private const int ExpectedPort2 = 2222;

        [OneTimeSetUp]
        public void OnetimeSetup()
        {
            _connectionFileContents = Resources.test_remotedesktopmanager_rdm;
            _deserializer = new RemoteDesktopManagerDeserializer();
            _connectionTreeModel = _deserializer.Deserialize(_connectionFileContents);
        }

        private ConnectionInfo GetFirstConnectionInfo()
        {
            var rootNode = _connectionTreeModel.RootNodes.First();
            var group1 = rootNode.Children.OfType<ContainerInfo>().First(node => node.Name == ExpectedGroupName1);
            var connection = group1.Children.First();
            return connection;
        }

        private ConnectionInfo GetSSHConnectionInfo()
        {
            var rootNode = _connectionTreeModel.RootNodes.First();
            var group2 = rootNode.Children.OfType<ContainerInfo>().First(node => node.Name == ExpectedGroupName2);
            var connection = group2.Children.First();
            return connection;
        }

        [Test]
        public void ConnectionTreeModelHasARootNode()
        {
            var numberOfRootNodes = _connectionTreeModel.RootNodes.Count;
            Assert.That(numberOfRootNodes, Is.GreaterThan(0));
        }

        [Test]
        public void RootNodeHasContents()
        {
            var rootNodeContents = _connectionTreeModel.RootNodes.First().Children;
            Assert.That(rootNodeContents, Is.Not.Empty);
        }

        [Test]
        public void AllSubRootFoldersImported()
        {
            var rootNode = _connectionTreeModel.RootNodes.First();
            var rootNodeContents = rootNode.Children.Count(node => node.Name == ExpectedGroupName1 || node.Name == ExpectedGroupName2);
            Assert.That(rootNodeContents, Is.EqualTo(2));
        }

        [Test]
        public void ConnectionDisplayNameImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.Name, Is.EqualTo(ExpectedName));
        }

        [Test]
        public void ConnectionHostnameImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.Hostname, Is.EqualTo(ExpectedHostname));
        }

        [Test]
        public void ConnectionUsernameImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.Username, Is.EqualTo(ExpectedUsername));
        }

        [Test]
        public void ConnectionDomainImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.Domain, Is.EqualTo(ExpectedDomain));
        }

        [Test]
        public void ConnectionProtocolSetToRdp()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.Protocol, Is.EqualTo(ProtocolType.RDP));
        }

        [Test]
        public void ConnectionUseConsoleSessionImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.UseConsoleSession, Is.EqualTo(ExpectedUseConsoleSession));
        }

        [Test]
        public void ConnectionPortImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.Port, Is.EqualTo(ExpectedPort));
        }

        [Test]
        public void ConnectionGatewayUsageMethodImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.RDGatewayUsageMethod, Is.EqualTo(ExpectedGatewayUsageMethod));
        }

        [Test]
        public void ConnectionGatewayHostnameImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.RDGatewayHostname, Is.EqualTo(ExpectedGatewayHostname));
        }

        [Test]
        public void ConnectionGatewayUsernameImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.RDGatewayUsername, Is.EqualTo(ExpectedGatewayUsername));
        }

        [Test]
        public void ConnectionGatewayDomainImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.RDGatewayDomain, Is.EqualTo(ExpectedGatewayDomain));
        }

        [Test]
        public void ConnectionResolutionImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.Resolution, Is.EqualTo(ExpectedRdpResolution));
        }

        [Test]
        public void ConnectionColorDepthImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.Colors, Is.EqualTo(ExpectedRdpColorDepth));
        }

        [Test]
        public void ConnectionAudioRedirectionImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.RedirectSound, Is.EqualTo(ExpectedAudioRedirection));
        }

        [Test]
        public void ConnectionKeyRedirectionImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.RedirectKeys, Is.EqualTo(ExpectedKeyRedirection));
        }

        [Test]
        public void ConnectionDriveRedirectionImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.RedirectDiskDrives, Is.EqualTo(ExpectedDriveRedirection));
        }

        [Test]
        public void ConnectionPortRedirectionImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.RedirectPorts, Is.EqualTo(ExpectedPortRedirection));
        }

        [Test]
        public void ConnectionPrinterRedirectionImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.RedirectPrinters, Is.EqualTo(ExpectedPrinterRedirection));
        }

        [Test]
        public void ConnectionSmartcardRedirectionImported()
        {
            var connection = GetFirstConnectionInfo();
            Assert.That(connection.RedirectSmartCards, Is.EqualTo(ExpectedSmartcardRedirection));
        }
        
        // [Test]
        // public void ConnectionPasswordImported()
        // {
        //     var connection = GetFirstConnectionInfo();
        //     Assert.That(connection.Password, Is.EqualTo(ExpectedPassword));
        // }
        
        // [Test]
        // public void ConnectionGatewayPasswordImported()
        // {
        //     var connection = GetFirstConnectionInfo();
        //     Assert.That(connection.RDGatewayPassword, Is.EqualTo(ExpectedGatewayPassword));
        // }

        [Test]
        public void Connection2DisplayNameImported()
        {
            var connection = GetSSHConnectionInfo();
            Assert.That(connection.Name, Is.EqualTo(ExpectedName2));
        }

        [Test]
        public void Connection2HostnameImported()
        {
            var connection = GetSSHConnectionInfo();
            Assert.That(connection.Hostname, Is.EqualTo(ExpectedHostname2));
        }

        [Test]
        public void Connection2UsernameImported()
        {
            var connection = GetSSHConnectionInfo();
            Assert.That(connection.Username, Is.EqualTo(ExpectedUsername2));
        }

        [Test]
        public void Connection2ProtocolSetToRdp()
        {
            var connection = GetSSHConnectionInfo();
            Assert.That(connection.Protocol, Is.EqualTo(ProtocolType.SSH2));
        }

        [Test]
        public void Connection2PortImported()
        {
            var connection = GetSSHConnectionInfo();
            Assert.That(connection.Port, Is.EqualTo(ExpectedPort2));
        }
    }
}