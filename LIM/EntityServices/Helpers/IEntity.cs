using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.EntityServices.Helpers
{
    public interface IEntity
    {
        public const string NEW_ID_STR = "new";

        string Id { get; set; }

        string WebUrl { get; set; }

    }
}
