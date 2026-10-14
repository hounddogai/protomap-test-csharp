namespace Acme.Workforce.Compensation.PayBand.Tier;

/// <summary>A rule a tiered pay band allocation must pass. Check returns its problems, or none.</summary>
public interface ITieredPayBandAllocationRule
{
    List<string> Check(TieredPayBandAllocation item);
}

public sealed record TieredPayBandAllocationSequenceRangeRule(int Min, int Max) : ITieredPayBandAllocationRule
{
    public List<string> Check(TieredPayBandAllocation item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredPayBandAllocationCodePrefixRule(string Prefix) : ITieredPayBandAllocationRule
{
    public List<string> Check(TieredPayBandAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPayBandAllocationPriorityRangeRule(int Min, int Max) : ITieredPayBandAllocationRule
{
    public List<string> Check(TieredPayBandAllocation item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tiered pay band allocation.</summary>
public sealed record TieredPayBandAllocationValidator(List<ITieredPayBandAllocationRule> Rules)
{
    public List<string> Validate(TieredPayBandAllocation item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(TieredPayBandAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPayBandAllocation> items)
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

/// <summary>Functions of tiered pay band allocation rules.</summary>
public static class TieredPayBandAllocationRules
{
    public static TieredPayBandAllocationValidator DefaultTieredPayBandAllocationValidator()
    {
        return new TieredPayBandAllocationValidator(
            new List<ITieredPayBandAllocationRule>
            {
                new TieredPayBandAllocationSequenceRangeRule(255, 1000),
                new TieredPayBandAllocationCodePrefixRule("X"),
                new TieredPayBandAllocationPriorityRangeRule(32, 32),
            });
    }
}
