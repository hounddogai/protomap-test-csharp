namespace Acme.Workforce.Talent.Pool.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pool entry must pass. Check returns its problems, or none.</summary>
public interface IPoolEntryRule
{
    List<string> Check(PoolEntry item);
}

public sealed record PoolEntryPriorityRangeRule(int Min, int Max) : IPoolEntryRule
{
    public List<string> Check(PoolEntry item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolEntryRequiredTimezoneRule(string Message) : IPoolEntryRule
{
    public List<string> Check(PoolEntry item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolEntryAllowedNameRule(List<string> Allowed) : IPoolEntryRule
{
    public List<string> Check(PoolEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool entry.</summary>
public sealed record PoolEntryValidator(List<IPoolEntryRule> Rules)
{
    public List<string> Validate(PoolEntry item)
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

    public void RequireValid(PoolEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolEntry> items)
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

/// <summary>Functions of pool entry rules.</summary>
public static class PoolEntryRules
{
    public static PoolEntryValidator DefaultPoolEntryValidator()
    {
        return new PoolEntryValidator(
            new List<IPoolEntryRule>
            {
                new PoolEntryPriorityRangeRule(64, 3600),
                new PoolEntryRequiredTimezoneRule("pool entry needs this field"),
                new PoolEntryAllowedNameRule(new List<string> { "capped", "capped" }),
            });
    }
}
