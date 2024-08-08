using Boostlingo.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Boostlingo.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IConfiguration _configuration;
        private string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["Postgres:ConnectionString"];
        }

        public async Task WriteDummyDataAsync(List<DummyModel> models)
        {
            var connectionString = _configuration["Postgres:ConnectionString"];
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            foreach (var model in models)
            {
                var sqlString = "INSERT INTO " +
                    "DummyData (FirstName, LastName, Language, Id, Bio, Version) " +
                    "VALUES (@firstName, @lastName, @language, @id, @bio, @version)";
                var insertCommand = new NpgsqlCommand(sqlString, connection);

                insertCommand.Parameters.AddWithValue("firstName", model.FirstName);
                insertCommand.Parameters.AddWithValue("lastName", model.LastName);
                insertCommand.Parameters.AddWithValue("language", model.Language);
                insertCommand.Parameters.AddWithValue("id", model.Id);
                insertCommand.Parameters.AddWithValue("bio", model.Bio);
                insertCommand.Parameters.AddWithValue("version", model.Version);

                await insertCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<DummyModel>> ReadDummyDataAsync()
        {
            var models = new List<DummyModel>();

            using var connection = new NpgsqlConnection(_connectionString);
            {
                await connection.OpenAsync();

                var commandText = "SELECT id, firstname, lastname, language, bio, version FROM DummyData";
                using var command = new NpgsqlCommand(commandText, connection);
                {
                    using var reader = await command.ExecuteReaderAsync();
                    {
                        while (await reader.ReadAsync())
                        {
                            var model = new DummyModel
                            {
                                Id = reader["id"].ToString(),
                                FirstName = reader["firstname"].ToString(),
                                LastName = reader["lastname"].ToString(),
                                Language = reader["language"].ToString(),
                                Bio = reader["bio"].ToString(),
                                Version = reader.GetDouble(reader.GetOrdinal("version"))
                            };

                            models.Add(model);
                        }
                    }
                }
            }

            return models;
        }

        public async Task ClearTableAsync(string tableName)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            {
                await connection.OpenAsync();
                var sql = $"DELETE FROM {tableName}";
                using var command = new NpgsqlCommand(sql, connection);
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
