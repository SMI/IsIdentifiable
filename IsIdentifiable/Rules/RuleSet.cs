using System;
using System.Collections.Generic;
using System.Text;

namespace IsIdentifiable.Rules
{
    public class RuleSet
    {
        public List<IsIdentifiableRule> BasicRules { get; set; } = new List<IsIdentifiableRule>();
        public List<SocketRule> SocketRules { get; set; } = new List<SocketRule>();
        public List<AllowlistRule> AllowlistRules { get; set; } = new List<AllowlistRule>();
        public List<ConsensusRule> ConsensusRules { get; set; } = new List<ConsensusRule>();
    }
}
