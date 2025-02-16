// using Microsoft.Data.Sqlite;
// using Microsoft.EntityFrameworkCore;
//
// namespace Illumination.Assembly;
//
// public sealed class ContextFactory : IDbContextFactory<Context>, IDisposable
// {
// 	private readonly String connectionId;
// 	private readonly SqliteConnection connection;
//
// 	public ContextFactory()
// 	{
// 		connectionId = GetHashCode().ToString();
// 		
// 		connection = new InMemorySqliteConnection(connectionId);
// 		connection.Open();
// 		
// 		using var context = new Context(connectionId);
// 		context.Database.EnsureCreated();
// 	}
//
// 	public void Dispose() => connection.Dispose();
//
// 	public Context CreateDbContext() => new(connectionId);
// }