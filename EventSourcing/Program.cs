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


            foreach (var e in eb.AllEvents) {
                Console.WriteLine(e.ToString());
            }
            string productPrice;
            productPrice = eb.Query<string>(new PriceQuery() { Target = p });
            Console.WriteLine(productPrice);

            eb.UndoLast();
            foreach (var e in eb.AllEvents)
            {
                Console.WriteLine(e.ToString());
            }

            productPrice = eb.Query<string>(new PriceQuery() { Target = p });
            Console.WriteLine(productPrice);
            Console.ReadLine();
            
        }
       
    }

    // Class 
    public class Products {

        EventBroker _eventBroker;
        private string productPrice = "0";
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

                //Log the events
               if(cac.Register == true) _eventBroker.AllEvents.Add(new PriceChangeEvent(this, this.productPrice, cac._price));

                productPrice = cac._price;
            }
        }
    }

    public class EventBroker {

        public IList<DomainEvent> AllEvents = new List<DomainEvent>();
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
        public void UndoLast() {

            var e = AllEvents.LastOrDefault();
            var pc = e as PriceChangeEvent;
            if (pc != null) {

                Command(new ChangePriceCommand(pc.Target, pc.oldPrice) { Register = false});
                AllEvents.Remove(e);
            }

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
        public bool Register = true;
    }
    public class ChangePriceCommand : Command {

        public Products _target;
        public string _price;
        public ChangePriceCommand(Products target, string price) {
            _target = target;
            _price = price;
        }
        
    }

    public class DomainEvent
    {
    }
    public class PriceChangeEvent : DomainEvent {

        public Products Target;
        public string oldPrice, newPrice;

        public PriceChangeEvent(Products target, string oldPrice, string newPrice)
        {
            Target = target;
            this.oldPrice = oldPrice;
            this.newPrice = newPrice;
        }
        public override string ToString()
        {
            return $"Price Changed from {oldPrice} to {newPrice}";
        }
    }
}
