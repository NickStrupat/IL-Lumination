using Microsoft.Data.Sqlite;

namespace Illumination.Assembly;

internal sealed class InMemorySqliteConnection(String connectionId)
	: SqliteConnection($"Data Source={connectionId};Mode=Memory;Cache=Shared");