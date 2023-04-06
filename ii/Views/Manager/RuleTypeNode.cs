using System;
using IsIdentifiable.Rules;
using System.Collections;
using System.Reflection;

namespace IsIdentifiable.Views.Manager;

internal class RuleTypeNode
{
    public RuleSetFileNode Parent { get; set; }
    private PropertyInfo? prop;

    public IList? Rules { get; internal set; }

    /// <summary>
    /// Creates a new instance 
    /// </summary>
    /// <param name="ruleSet"></param>
    /// <param name="ruleProperty"></param>
    public RuleTypeNode(RuleSetFileNode ruleSet, string ruleProperty)
    {
        prop = typeof(RuleSet).GetProperty(ruleProperty);
        if(prop == null)
        {
            throw new ArgumentException($"No property called {ruleProperty} exists on Type RuleSet");
        }

        Parent = ruleSet;
        Rules = (IList?)prop.GetValue(ruleSet.GetRuleSet());
    }

    public override string ToString()
    {
        return prop?.Name ?? "Unknown Property";
    }
}