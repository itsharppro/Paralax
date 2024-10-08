namespace Paralax.Secrets.Vault
{
    public interface IKeyValueSecrets
    {
        Task<T> GetDefaultAsync<T>();
        Task<IDictionary<string, object>> GetDefaultAsync();
        Task<T> GetAsync<T>(string path);
        Task<IDictionary<string, object>> GetAsync(string path);
    }
}