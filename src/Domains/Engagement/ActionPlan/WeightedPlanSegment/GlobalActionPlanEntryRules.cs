namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

/// <summary>A rule a global action plan entry must pass. Check returns its problems, or none.</summary>
public interface IGlobalActionPlanEntryRule
{
    List<string> Check(GlobalActionPlanEntry item);
}

public sealed record GlobalActionPlanEntryRequiredSalaryBandRule(string Message) : IGlobalActionPlanEntryRule
{
    public List<string> Check(GlobalActionPlanEntry item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalActionPlanEntryCostCenterLengthRule(int MaxLength) : IGlobalActionPlanEntryRule
{
    public List<string> Check(GlobalActionPlanEntry item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalActionPlanEntryCreatedAtRangeRule(int Min, int Max) : IGlobalActionPlanEntryRule
{
    public List<string> Check(GlobalActionPlanEntry item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalActionPlanEntryCodePrefixRule(string Prefix) : IGlobalActionPlanEntryRule
{
    public List<string> Check(GlobalActionPlanEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global action plan entry.</summary>
public sealed record GlobalActionPlanEntryValidator(List<IGlobalActionPlanEntryRule> Rules)
{
    public List<string> Validate(GlobalActionPlanEntry item)
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

    public void RequireValid(GlobalActionPlanEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalActionPlanEntry> items)
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

/// <summary>Functions of global action plan entry rules.</summary>
public static class GlobalActionPlanEntryRules
{
    public static GlobalActionPlanEntryValidator DefaultGlobalActionPlanEntryValidator()
    {
        return new GlobalActionPlanEntryValidator(
            new List<IGlobalActionPlanEntryRule>
            {
                new GlobalActionPlanEntryRequiredSalaryBandRule("global action plan entry needs a value"),
                new GlobalActionPlanEntryCostCenterLengthRule(255),
                new GlobalActionPlanEntryCreatedAtRangeRule(16, 32),
                new GlobalActionPlanEntryCodePrefixRule("OPS"),
            });
    }
}
