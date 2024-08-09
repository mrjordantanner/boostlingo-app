using Boostlingo.Models;

namespace Boostlingo.Services
{
    public interface IDatabaseService
    {
        public Task<bool> WriteDummyDataAsync(List<DummyModel> models);
        public Task<List<DummyModel>> ReadDummyDataAsync();
        public Task ClearTableAsync();

    }
}
