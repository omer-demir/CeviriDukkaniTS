using RabbitMQ.Client;
using Tangent.CeviriDukkani.Event.OrderEvents;
using Tangent.CeviriDukkani.Messaging.Consumer;

namespace Ts.Api {
    public class TsEventProjection {
        private readonly Settings _settings;
        private readonly RabbitMqSubscription _consumer;

        public TsEventProjection(IConnection connection, Settings settings) {
            _settings = settings;
            _consumer = new RabbitMqSubscription(connection, settings.RabbitExchangeName);
            _consumer
                .WithAppName("ts-projection")
                .WithEvent<CreateOrderDetailEvent>(Handle);
        }

        public void Start() {
            _consumer.Subscribe();
        }

        public void Stop() {
            _consumer.StopSubscriptionTasks();
        }

        public void Handle(CreateOrderDetailEvent orderDetailEvent) {
        }
    }
}