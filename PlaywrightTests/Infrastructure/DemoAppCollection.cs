namespace PlaywrightTests.Infrastructure;

/// <summary>
/// xUnit collection that shares a single <see cref="DemoAppFactory"/> instance
/// across all test classes so Kestrel boots exactly once.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DemoAppCollection : ICollectionFixture<DemoAppFactory>
{
    public const string Name = "Demo app";
}
