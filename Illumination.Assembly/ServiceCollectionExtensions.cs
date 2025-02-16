using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Illumination.Assembly;

public static class ServiceCollectionExtensions
{
	public static void AddAssemblyStuff(
		this IServiceCollection services,
		Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
		Int32 dbContextPoolSize = DbContextPool<DbContext>.DefaultPoolSize
		)
	{
		var connectionId = Guid.NewGuid().ToString("N");
		services.AddTransient(ConnectionFactory);
		services.AddPooledDbContextFactory<Context>((provider, builder) =>
		{
			optionsAction(provider, builder);
			builder.UseSqlite(provider.GetRequiredService<InMemorySqliteConnection>());
		});
		
		services.AddSingleton<IDbContextFactory<DbContext>>(
			sp =>
			{
				var factory = sp.GetRequiredService<IDbContextFactory<Context>>();
				return new DbContextFactoryAdapter<Context>(factory);
			});
		
		return;

		InMemorySqliteConnection ConnectionFactory(IServiceProvider arg)
		{
			var connection = new InMemorySqliteConnection(connectionId);
			connection.Open();
			return connection;
		}
	}
	
	private sealed class DbContextFactoryAdapter<TDbContext>(IDbContextFactory<TDbContext> factory)
		: IDbContextFactory<DbContext> where TDbContext : DbContext
	{
		DbContext IDbContextFactory<DbContext>.CreateDbContext() =>
			factory.CreateDbContext();

		async Task<DbContext> IDbContextFactory<DbContext>.CreateDbContextAsync(CancellationToken ct) =>
			await factory.CreateDbContextAsync(ct);
	}
}

// public static class ServiceProviderExtensions
// {
// 	public static 
// }