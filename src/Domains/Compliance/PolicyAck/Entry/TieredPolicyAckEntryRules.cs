namespace Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>A rule a tiered policy ack entry must pass. Check returns its problems, or none.</summary>
public interface ITieredPolicyAckEntryRule
{
    List<string> Check(TieredPolicyAckEntry item);
}

public sealed record TieredPolicyAckEntryRequiredTitleRule(string Message) : ITieredPolicyAckEntryRule
{
    public List<string> Check(TieredPolicyAckEntry item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyAckEntryRequiredNameRule(string Message) : ITieredPolicyAckEntryRule
{
    public List<string> Check(TieredPolicyAckEntry item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyAckEntryRequiredCodeRule(string Message) : ITieredPolicyAckEntryRule
{
    public List<string> Check(TieredPolicyAckEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered policy ack entry.</summary>
public sealed record TieredPolicyAckEntryValidator(List<ITieredPolicyAckEntryRule> Rules)
{
    public List<string> Validate(TieredPolicyAckEntry item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(TieredPolicyAckEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPolicyAckEntry> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of tiered policy ack entry rules.</summary>
public static class TieredPolicyAckEntryRules
{
    public static TieredPolicyAckEntryValidator DefaultTieredPolicyAckEntryValidator()
    {
        return new TieredPolicyAckEntryValidator(
            new List<ITieredPolicyAckEntryRule>
            {
                new TieredPolicyAckEntryRequiredTitleRule("tiered policy ack entry needs a value"),
                new TieredPolicyAckEntryRequiredNameRule("tiered policy ack entry needs this field"),
                new TieredPolicyAckEntryRequiredCodeRule("tiered policy ack entry needs an entry"),
            });
    }
}
