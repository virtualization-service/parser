using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Parser.Processors
{
    public class MessageConsumer
    {

        MessageExtractor _extractor;

        private IModel _channel;

        public MessageConsumer(MessageExtractor extractor)
        {
            _extractor = extractor;
        }

        public void Register(ConnectionFactory factory)
        {
            var connection = factory.CreateConnection();

            _channel = connection.CreateModel();

            _channel.ExchangeDeclare("configuration", type: "topic", durable: true);
            _channel.QueueDeclare("parser");
            _channel.QueueBind("parser","configuration", "configuration.train");

            _channel.ExchangeDeclare("virtualization", type: "topic", durable: true);
            _channel.QueueDeclare("vir_parser");
            _channel.QueueBind("vir_parser","virtualization", "virtualization.begin");

            _channel.ConfirmSelect();

            _channel.BasicAcks += ChannelBasicAck;

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (consumerModel ,ea) =>
            {
                try
                {
                    _extractor.ConsumeMessage(ea, factory);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error {ex}");
                }
            };

            _channel.BasicQos(0,10000, false);
            _channel.BasicConsume("parser", true, consumer: consumer);
            _channel.BasicConsume("vir_parser", true, consumer: consumer);
        }

        public void DeRegister(ConnectionFactory factory)
        {
            if(_channel != null) _channel.Close();
        }

        private void ChannelBasicAck(object sender, BasicAckEventArgs e)
        {
            Console.WriteLine($"Received Acknowledgement {e.DeliveryTag}");
        }
    }
}