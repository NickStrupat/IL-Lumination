using Microsoft.EntityFrameworkCore;

namespace Illumination.Assembly;

internal class Context(DbContextOptions options) : DbContext(options)
{
	protected override void OnModelCreating(ModelBuilder mb)
	{
		var defType = typeof(Definition);
		var det = mb.Entity(defType);
		// const String keyName = "Id";
		// det.Property(typeof(Int32), keyName);
		// det.HasKey(keyName);
		det.UseTphMappingStrategy();
		var defTypes = defType.Assembly.GetTypes().Where(x => x.IsSubclassOf(defType) && x != defType);
		foreach (var type in defTypes)
		{
			var etb = mb.Entity(type);
			//etb.UseTptMappingStrategy();
			// if (type.BaseType == defType)
			// {
			// 	etb.Property(typeof(Int32), keyName);
			// 	etb.HasKey(keyName);
			// }
		}

		var namedDefEtb = mb.Entity<Definition>();
		namedDefEtb.HasIndex(x => x.Name);
		namedDefEtb.HasIndex(x => new { x.ParentId, x.Name }).IsUnique();
		namedDefEtb.HasIndex(x => x.ParentId);
	}
}