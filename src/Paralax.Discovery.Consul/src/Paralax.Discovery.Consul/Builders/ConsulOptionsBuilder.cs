using System.Collections.Generic;

namespace Paralax.Discovery.Consul.Builders
{
    public class ConsulOptionsBuilder : IConsulOptionsBuilder
    {
        private readonly ConsulOptions _options = new();

        public IConsulOptionsBuilder Enable(bool enabled)
        {
            _options.Enabled = enabled;
            return this;
        }

        public IConsulOptionsBuilder WithUrl(string url)
        {
            _options.Url = url;
            return this;
        }

        public IConsulOptionsBuilder WithService(string service)
        {
            _options.Service = service;
            return this;
        }

        public IConsulOptionsBuilder WithAddress(string address)
        {
            _options.Address = address;
            return this;
        }

        public IConsulOptionsBuilder WithPort(int port)
        {
            _options.Port = port;
            return this;
        }

        public IConsulOptionsBuilder WithTags(List<string> tags)
        {
            _options.Tags = tags;
            return this;
        }

        public IConsulOptionsBuilder WithMeta(IDictionary<string, string> meta)
        {
            _options.Meta = meta;
            return this;
        }

        public IConsulOptionsBuilder WithEnabledPing(bool pingEnabled)
        {
            _options.PingEnabled = pingEnabled;
            return this;
        }

        public IConsulOptionsBuilder WithPingEndpoint(string pingEndpoint)
        {
            _options.PingEndpoint = pingEndpoint;
            return this;
        }

        public IConsulOptionsBuilder WithPingInterval(string pingInterval)
        {
            _options.PingInterval = pingInterval;
            return this;
        }

        public IConsulOptionsBuilder WithRemoveAfterInterval(string removeAfterInterval)
        {
            _options.RemoveAfterInterval = removeAfterInterval;
            return this;
        }

        public IConsulOptionsBuilder WithSkipLocalhostDockerDnsReplace(bool skipLocalhostDockerDnsReplace)
        {
            _options.SkipLocalhostDockerDnsReplace = skipLocalhostDockerDnsReplace;
            return this;
        }

        public IConsulOptionsBuilder WithEnableTagOverride(bool enableTagOverride)
        {
            _options.EnableTagOverride = enableTagOverride;
            return this;
        }

        public IConsulOptionsBuilder WithMaxRetryAttempts(int maxRetryAttempts)
        {
            _options.MaxRetryAttempts = maxRetryAttempts;
            return this;
        }

        public IConsulOptionsBuilder WithRetryDelay(string retryDelay)
        {
            _options.RetryDelay = retryDelay;
            return this;
        }

        public IConsulOptionsBuilder WithRegistrationTimeout(string registrationTimeout)
        {
            _options.RegistrationTimeout = registrationTimeout;
            return this;
        }

        public IConsulOptionsBuilder WithAutoDeregister(bool autoDeregister)
        {
            _options.AutoDeregister = autoDeregister;
            return this;
        }

        public IConsulOptionsBuilder WithCheckTTL(string checkTTL)
        {
            _options.CheckTTL = checkTTL;
            return this;
        }

        public IConsulOptionsBuilder WithEnableTlsVerification(bool enableTlsVerification)
        {
            _options.EnableTlsVerification = enableTlsVerification;
            return this;
        }

        public IConsulOptionsBuilder WithTlsCertPath(string tlsCertPath)
        {
            _options.TlsCertPath = tlsCertPath;
            return this;
        }

        public IConsulOptionsBuilder WithAclToken(string aclToken)
        {
            _options.AclToken = aclToken;
            return this;
        }

        public IConsulOptionsBuilder WithDebug(bool debug)
        {
            _options.Debug = debug;
            return this;
        }

        public IConsulOptionsBuilder WithConnectOptions(bool connectEnabled)
        {
            _options.Connect.Enabled = connectEnabled;
            return this;
        }

        public ConsulOptions Build()
        {
            return _options;
        }
    }
}
