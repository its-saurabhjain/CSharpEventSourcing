using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EventSourcing
{
    
    class Program
    {

        
        static void Main(string[] args)
        {
            EventBroker eb = new  EventBroker();
            Products p = new Products(eb);
            eb.Command(new ChangePriceCommand(p, "120"));

            string productPrice = eb.Query<string>(new PriceQuery() { Target = p });
            Console.WriteLine(productPrice);

            Console.ReadLine();
            
        }
       
    }

    // Class 
    public class Products {

        EventBroker _eventBroker;
        private string productPrice;
        public Products(EventBroker eventBroker) {

            this._eventBroker = eventBroker;
            _eventBroker.commands += BrokerOnCommand;
            _eventBroker.querys += BrokerOnQuery;
        }

        private void BrokerOnQuery(object sender, Query e)
        {
            var agq = e as PriceQuery;
            if (agq != null && agq.Target == this)
            {
                agq.Result = productPrice;
            }
        }

        private void BrokerOnCommand(object sender, Command e)
        {
            var cac = e as ChangePriceCommand;
            if (cac != null && cac._target == this) {

                productPrice = cac._price;
            }
        }
    }

    public class EventBroker {

        IList<DomainEvent> AllEvents = new List<DomainEvent>();
        //Command Events
        public event EventHandler<Command> commands;
        public event EventHandler<Query> querys;
        public void Command(Command c) {
            commands?.Invoke(this, c);
        }
        public T Query<T>(Query q)
        {
            querys?.Invoke(this, q);
            return (T) q.Result;
        }
    }

    public class Query: EventArgs
    {
        public object Result;
    }
    public class PriceQuery : Query
    {
        public Products Target;
        public string Price;

    }

    public class Command: EventArgs
    {
    }
    public class ChangePriceCommand : Command {

        public Products _target;
        public string _price;
        public ChangePriceCommand(Products target, string price) {
            _target = target;
            _price = price;
        }
    }

    internal class DomainEvent
    {
    }
}
