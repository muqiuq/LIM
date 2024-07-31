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

        // Entity has been changed localy and need to be synced
        public bool Updated { get; set; } = false;

        public bool RequiresLogEntry { get; set; } = false;

        public DateTime? LockedSince { get; set; } = null;

        public EntityStateWrapper(T t)
        {
            this.Entity = t;
        }

        public EntityStateWrapper()
        {

        }

    }
}
