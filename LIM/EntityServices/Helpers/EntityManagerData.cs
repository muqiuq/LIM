using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.EntityServices.Helpers
{
    public class EntityManagerData<T> where T : IEntity
    {
        public List<EntityStateWrapper<T>> Items { get; set; }

        public Dictionary<string, List<string>> Choices { get; set; }

    }
}
