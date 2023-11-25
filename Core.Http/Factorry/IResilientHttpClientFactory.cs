namespace Core.Http.Factorry
{
    internal interface IResilientHttpClientFactory
    {
        ResilientHttpClient CreateResilientHttpClient();
    }
}