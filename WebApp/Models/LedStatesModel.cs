using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class SingleLedModel
    {
        public string Type { get; set; }

        public IList<DateTime> Times { get; private set; } = new List<DateTime>();

        public IList<int> States { get; private set; } = new List<int>();
    }

    public class LedsModel
    {
        public SingleLedModel Red { get; private set; } = new SingleLedModel() { Type = "red" };

        public SingleLedModel Green { get; private set; } = new SingleLedModel() { Type = "green" };
    }
}