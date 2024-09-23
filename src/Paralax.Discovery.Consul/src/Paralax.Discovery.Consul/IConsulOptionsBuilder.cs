namespace Paralax.Discovery.Consul
{
    public interface IConsulOptionsBuilder
    {
        IConsulOptionsBuilder Enable(bool enabled);
        IConsulOptionsBuilder WithUrl(string url);
        IConsulOptionsBuilder WithService(string service);
        IConsulOptionsBuilder WithAddress(string address);
        IConsulOptionsBuilder WithPort(int port);
        IConsulOptionsBuilder WithTags(List<string> tags);
        IConsulOptionsBuilder WithMeta(IDictionary<string, string> meta);
        IConsulOptionsBuilder WithEnabledPing(bool pingEnabled);
        IConsulOptionsBuilder WithPingEndpoint(string pingEndpoint);
        IConsulOptionsBuilder WithPingInterval(string pingInterval);
        IConsulOptionsBuilder WithRemoveAfterInterval(string removeAfterInterval);
        IConsulOptionsBuilder WithSkipLocalhostDockerDnsReplace(bool skipLocalhostDockerDnsReplace);
        IConsulOptionsBuilder WithEnableTagOverride(bool enableTagOverride);
        IConsulOptionsBuilder WithMaxRetryAttempts(int maxRetryAttempts);
        IConsulOptionsBuilder WithRetryDelay(string retryDelay);
        IConsulOptionsBuilder WithRegistrationTimeout(string registrationTimeout);
        IConsulOptionsBuilder WithAutoDeregister(bool autoDeregister);
        IConsulOptionsBuilder WithCheckTTL(string checkTTL);
        IConsulOptionsBuilder WithEnableTlsVerification(bool enableTlsVerification);
        IConsulOptionsBuilder WithTlsCertPath(string tlsCertPath);
        IConsulOptionsBuilder WithAclToken(string aclToken);
        IConsulOptionsBuilder WithDebug(bool debug);
        IConsulOptionsBuilder WithConnectOptions(bool connectEnabled);
        ConsulOptions Build();
    }
}
