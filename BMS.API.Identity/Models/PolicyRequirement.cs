using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Models
{
    public class PolicyRequirement
    {
        public int PolicyId { get; set; }

        public Policy Policy { get; set; }

        public int RequirementId { get; set; }

        public Requirement Requirement { get; set; }
    }
}
