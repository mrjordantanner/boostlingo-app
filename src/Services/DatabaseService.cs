using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Boostlingo.Models;
using Npgsql;


namespace Boostlingo.Services
{
    public class DatabaseService
    {
        //private readonly string _connectionString;

        //public DatabaseService(string connectionString)
        //{
        //    _connectionString = connectionString;
        //}

        public async Task InsertDataAsync(List<DummyModel> models)
        {
            // TODO store this in config
            var _connectionString = "Host=localhost;Port=5432;Username=test-user;Password=boostlingo;Database=test-db";

            // Open a new connection
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Write each model to the database
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
    }
}
