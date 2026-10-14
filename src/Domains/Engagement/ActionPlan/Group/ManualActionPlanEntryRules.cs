namespace Acme.Workforce.Engagement.ActionPlan.Group;

/// <summary>A rule a manual action plan entry must pass. Check returns its problems, or none.</summary>
public interface IManualActionPlanEntryRule
{
    List<string> Check(ManualActionPlanEntry item);
}

public sealed record ManualActionPlanEntryCodePrefixRule(string Prefix) : IManualActionPlanEntryRule
{
    public List<string> Check(ManualActionPlanEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualActionPlanEntryUpdatedAtRangeRule(int Min, int Max) : IManualActionPlanEntryRule
{
    public List<string> Check(ManualActionPlanEntry item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualActionPlanEntryRequiredExternalRefRule(string Message) : IManualActionPlanEntryRule
{
    public List<string> Check(ManualActionPlanEntry item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualActionPlanEntryRequiredCodeRule(string Message) : IManualActionPlanEntryRule
{
    public List<string> Check(ManualActionPlanEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualActionPlanEntryWeightRangeRule(double Min, double Max) : IManualActionPlanEntryRule
{
    public List<string> Check(ManualActionPlanEntry item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a manual action plan entry.</summary>
public sealed record ManualActionPlanEntryValidator(List<IManualActionPlanEntryRule> Rules)
{
    public List<string> Validate(ManualActionPlanEntry item)
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

    public void RequireValid(ManualActionPlanEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualActionPlanEntry> items)
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

/// <summary>Functions of manual action plan entry rules.</summary>
public static class ManualActionPlanEntryRules
{
    public static ManualActionPlanEntryValidator DefaultManualActionPlanEntryValidator()
    {
        return new ManualActionPlanEntryValidator(
            new List<IManualActionPlanEntryRule>
            {
                new ManualActionPlanEntryCodePrefixRule("REF"),
                new ManualActionPlanEntryUpdatedAtRangeRule(32, 1000),
                new ManualActionPlanEntryRequiredExternalRefRule("manual action plan entry needs this field"),
                new ManualActionPlanEntryRequiredCodeRule("manual action plan entry needs an entry"),
                new ManualActionPlanEntryWeightRangeRule(9.1, 3393.5),
            });
    }
}
