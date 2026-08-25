namespace RoyalVillaWeb
{
    public static class SD
    {
        public enum ApiType
        {
            GET, 
            POST, 
            PUT, 
            DELETE, 
            TRACE,
        }

        public const string SessionToken = "jwtSession";
    }
}
