using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.CartService.Application.Common.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;
}
