//A simple chat application
//The App takes in user input and sends in to RabbitMQ
//Use a unique Queue
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ChatApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://chat:chat@locahhost:5672");

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var exchangeName = "chat";
            var queueName = Guid.NewGuid().ToString();

            channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);
            channel.QueueDeclare(queueName,true,true,true);
            channel.QueueBind(queueName,exchangeName,"");
            //subcribe to incoming messages

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, eventArgs) =>
            {
                //text received

                var text = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                Console.WriteLine( text);

            };

            //start listening
            channel.BasicConsume(queueName, true, consumer);
            //getting input
            var input = Console.ReadLine();

            while(input != null)
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                channel.BasicPublish(exchangeName, "", null, bytes);
                input = Console.ReadLine();
            }
            channel.Close();
            connection.Close();


        }
    }
}