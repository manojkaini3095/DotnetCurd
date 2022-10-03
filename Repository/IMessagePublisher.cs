using System.Threading.Tasks;

namespace DotnetCurd.Repository
{
    public interface IMessagePublisher
    {
        Task PublisherAsync<T>(T request);
    }
}
