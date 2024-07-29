using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.EntityServices.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MsListColumn : Attribute
    {

        public MsListColumn(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
