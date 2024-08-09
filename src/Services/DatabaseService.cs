using Boostlingo.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;

namespace Boostlingo.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseService> _logger;

        private readonly string _connectionString;
        private readonly string _tableName;

        public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = configuration["Postgres:ConnectionString"];
            _tableName = configuration["Postgres:TableName"].ToUpper();

            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogError("Configuration error: ConnectionString not found.");
            }

            if (string.IsNullOrEmpty(_tableName))
            {
                _logger.LogError("Configuration error: TableName not found.");
            }
        }

        // Use a transaction to write to the table, adhering to ACID principles
        public async Task<bool> WriteDummyDataAsync(List<DummyModel> models)
        {
            using var connection = await OpenConnectionAsync();
            if (connection == null) return false;

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Writing data to table {_tableName}...", _tableName);
                foreach (var model in models)
                {
                    var sqlString = $"INSERT INTO {_tableName} (FirstName, LastName, Language, Id, Bio, Version) " +
                                    "VALUES (@firstName, @lastName, @language, @id, @bio, @version)";

                    using var command = new NpgsqlCommand(sqlString, connection, transaction);
                    command.Parameters.AddWithValue("@firstName", model.FirstName);
                    command.Parameters.AddWithValue("@lastName", model.LastName);
                    command.Parameters.AddWithValue("@language", model.Language);
                    command.Parameters.AddWithValue("@id", model.Id);
                    command.Parameters.AddWithValue("@bio", model.Bio);
                    command.Parameters.AddWithValue("@version", model.Version);

                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                _logger.LogInformation($"Write Transaction completed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error writing to database table. Transaction is being rolled back. {ex}", ex);
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<DummyModel>> ReadDummyDataAsync()
        {
            var models = new List<DummyModel>();

            using var connection = await OpenConnectionAsync();
            if (connection == null) return null;

            var sqlString = $"SELECT id, firstname, lastname, language, bio, version FROM {_tableName}";
            using var command = new NpgsqlCommand(sqlString, connection);
            {
                _logger.LogInformation("Reading data from table {_tableName}...", _tableName);
                using var reader = await command.ExecuteReaderAsync();
                {
                    // For efficiency, cache the index of each column before iterating through the result set
                    var idIndex = reader.GetOrdinal("id");
                    var firstNameIndex = reader.GetOrdinal("firstname");
                    var lastNameIndex = reader.GetOrdinal("lastname");
                    var languageIndex = reader.GetOrdinal("language");
                    var bioIndex = reader.GetOrdinal("bio");
                    var versionIndex = reader.GetOrdinal("version");

                    // While ReadAsync() returns true, there are more rows to read, so advance to the next one
                    while (await reader.ReadAsync())
                    {
                        // Map each row to a new DummyModel instance
                        var model = new DummyModel
                        {
                            Id = reader.GetString(idIndex),
                            FirstName = reader.GetString(firstNameIndex),
                            LastName = reader.GetString(lastNameIndex),
                            Language = reader.GetString(languageIndex),
                            Bio = reader.GetString(bioIndex),
                            Version = reader.GetDouble(versionIndex)
                        };

                        models.Add(model);
                    }
                }
            }

            return models;
        }

        private async Task<NpgsqlConnection> OpenConnectionAsync()
        {
            if (string.IsNullOrEmpty(_connectionString)) return null;

            // Use Polly for retry logic when establishing connection to the database
            var policy = Policy
                .Handle<NpgsqlException>()
                .WaitAndRetryAsync(
                    retryCount: 3,

                    // Exponential backoff gradually gives the database service more time to recover
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} due to: {exception.Message}");
                    });

            var connection = new NpgsqlConnection(_connectionString);

            _logger.LogInformation("Opening new database connection...");
            try
            {
                await policy.ExecuteAsync(async () =>
                {
                    await connection.OpenAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Connecting to database failed.");
                return null;
            }

            return connection;
        }

        public async Task ClearTableAsync()
        {
            using var connection = await OpenConnectionAsync();
            {
                var sql = $"DELETE FROM {_tableName}";
                using var command = new NpgsqlCommand(sql, connection);
                {
                    try 
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error clearing table {tableName}. {ex}", _tableName, ex);
                    }
                }
            }
        }


    }
}
