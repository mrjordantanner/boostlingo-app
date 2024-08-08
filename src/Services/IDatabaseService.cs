using Boostlingo.Models;

namespace Boostlingo.Services
{
    public interface IDatabaseService
    {
        public Task WriteDummyDataAsync(List<DummyModel> models);
        public Task<List<DummyModel>> ReadDummyDataAsync();
        public Task ClearTableAsync(string tableName);

    }
}
