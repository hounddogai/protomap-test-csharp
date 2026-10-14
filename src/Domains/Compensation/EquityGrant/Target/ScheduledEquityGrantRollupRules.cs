namespace Acme.Workforce.Compensation.EquityGrant.Target;

/// <summary>A rule a scheduled equity grant rollup must pass. Check returns its problems, or none.</summary>
public interface IScheduledEquityGrantRollupRule
{
    List<string> Check(ScheduledEquityGrantRollup item);
}

public sealed record ScheduledEquityGrantRollupQuantityRangeRule(int Min, int Max) : IScheduledEquityGrantRollupRule
{
    public List<string> Check(ScheduledEquityGrantRollup item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledEquityGrantRollupCodePrefixRule(string Prefix) : IScheduledEquityGrantRollupRule
{
    public List<string> Check(ScheduledEquityGrantRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantRollupRequiredCodeRule(string Message) : IScheduledEquityGrantRollupRule
{
    public List<string> Check(ScheduledEquityGrantRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled equity grant rollup.</summary>
public sealed record ScheduledEquityGrantRollupValidator(List<IScheduledEquityGrantRollupRule> Rules)
{
    public List<string> Validate(ScheduledEquityGrantRollup item)
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

    public void RequireValid(ScheduledEquityGrantRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledEquityGrantRollup> items)
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

/// <summary>Functions of scheduled equity grant rollup rules.</summary>
public static class ScheduledEquityGrantRollupRules
{
    public static ScheduledEquityGrantRollupValidator DefaultScheduledEquityGrantRollupValidator()
    {
        return new ScheduledEquityGrantRollupValidator(
            new List<IScheduledEquityGrantRollupRule>
            {
                new ScheduledEquityGrantRollupQuantityRangeRule(32, 64),
                new ScheduledEquityGrantRollupCodePrefixRule("Q"),
                new ScheduledEquityGrantRollupRequiredCodeRule("scheduled equity grant rollup needs an entry"),
            });
    }
}
