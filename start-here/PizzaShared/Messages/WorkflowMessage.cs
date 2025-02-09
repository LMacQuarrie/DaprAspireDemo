using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaShared.Messages
{
    public abstract class WorkflowMessage
    {
        public string WorkflowId { get; set; }
    }

    public class CustomerDto
    {
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required string Phone { get; set; }
    }
}
