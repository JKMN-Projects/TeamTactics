using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTactics.Domain.Common
{
    public class Entity
    {
        public int Id { get; private set; }

        protected Entity() { }
        protected Entity(int id) { this.Id = id; }

        public void SetId(int newId) => Id = newId;
    }
}
