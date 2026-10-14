namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pool summary must pass. Check returns its problems, or none.</summary>
public interface IPoolSummaryRule
{
    List<string> Check(PoolSummary item);
}

public sealed record PoolSummaryRetriesRangeRule(int Min, int Max) : IPoolSummaryRule
{
    public List<string> Check(PoolSummary item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PoolSummaryAllowedNameRule(List<string> Allowed) : IPoolSummaryRule
{
    public List<string> Check(PoolSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is expired" };
        }
        return new List<string>();
    }
}

public sealed record PoolSummaryRequiredCodeRule(string Message) : IPoolSummaryRule
{
    public List<string> Check(PoolSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool summary.</summary>
public sealed record PoolSummaryValidator(List<IPoolSummaryRule> Rules)
{
    public List<string> Validate(PoolSummary item)
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

    public void RequireValid(PoolSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolSummary> items)
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

/// <summary>Functions of pool summary rules.</summary>
public static class PoolSummaryRules
{
    public static PoolSummaryValidator DefaultPoolSummaryValidator()
    {
        return new PoolSummaryValidator(
            new List<IPoolSummaryRule>
            {
                new PoolSummaryRetriesRangeRule(120, 86400),
                new PoolSummaryAllowedNameRule(new List<string> { "adaptive", "batched", "seasonal" }),
                new PoolSummaryRequiredCodeRule("pool summary needs an entry"),
            });
    }
}
