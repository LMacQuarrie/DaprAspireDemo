using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShared.Messages.Delivery
{
    public class DeliveryMessage : WorkflowMessage
    {
        public string OrderId { get; set; }
        public string PizzaType { get; set; }
        public string Size { get; set; }
        public CustomerDto Customer { get; set; }
    }

    public class DeliveryResultMessage : DeliveryMessage
    {
        public string Status { get; set; }
        public string? Error { get; set; }
    }
}
