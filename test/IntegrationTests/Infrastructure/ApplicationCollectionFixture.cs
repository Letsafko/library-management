using Xunit;

namespace IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class ApplicationCollectionFixture : ICollectionFixture<ApplicationFixture>
{
    internal const string Name = "Library Fixture Collection";
}