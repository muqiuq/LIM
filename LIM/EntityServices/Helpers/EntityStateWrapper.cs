using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.EntityServices.Helpers
{
    public class EntityStateWrapper<T> where T : IEntity
    {
        public T Entity { get; set; }

        public bool Updated { get; set; } = false;

        public EntityStateWrapper(T t)
        {
            this.Entity = t;
        }

        public EntityStateWrapper()
        {

        }

    }
}
