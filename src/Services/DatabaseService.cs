using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Boostlingo.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using Npgsql;


namespace Boostlingo.Services
{
    public class DatabaseService
    {
        //private readonly string _connectionString;

        //public DatabaseService(IConfiguration configuration)
        //{
        //    _connectionString = configuration.GetConnectionString("DefaultConnection");
        //}

        public async Task WriteDummyDataAsync(List<DummyModel> models)
        {
            // TODO store this in config
            var _connectionString = "Host=localhost;Port=5432;Username=test-user;Password=boostlingo;Database=test-db";

            // Open a new connection
            using var connection = new NpgsqlConnection(_connectionString);
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

            // TODO store this in config
            var _connectionString = "Host=localhost;Port=5432;Username=test-user;Password=boostlingo;Database=test-db";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var commandText = "SELECT id, firstname, lastname, language, bio, version FROM DummyData";
                using (var command = new NpgsqlCommand(commandText, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
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
            // TODO store this in config
            var _connectionString = "Host=localhost;Port=5432;Username=test-user;Password=boostlingo;Database=test-db";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = $"DELETE FROM {tableName}";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
