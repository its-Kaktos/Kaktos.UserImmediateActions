namespace UserImmediateActions
{
    public interface IUserActionStoreKeyGenerator
    {
        /// <summary>
        /// Generates key based on <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <returns>Generated key.</returns>
        string GenerateKey(string userId);

        /// <summary>
        /// Generates key based on <paramref name="userId"/> and <paramref name="userAgent"/> and <paramref name="userIp"/>.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <param name="userAgent">Request user agent.</param>
        /// <param name="userIp">User ip address.</param>
        /// <returns>Generated key.</returns>
        string GenerateKey(string userId, string userAgent, string userIp);
    }
}