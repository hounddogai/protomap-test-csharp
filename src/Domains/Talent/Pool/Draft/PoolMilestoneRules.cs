namespace Acme.Workforce.Talent.Pool.Draft;

/// <summary>A rule a pool milestone must pass. Check returns its problems, or none.</summary>
public interface IPoolMilestoneRule
{
    List<string> Check(PoolMilestone item);
}

public sealed record PoolMilestoneRequiredNotesRule(string Message) : IPoolMilestoneRule
{
    public List<string> Check(PoolMilestone item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolMilestoneRequiredCodeRule(string Message) : IPoolMilestoneRule
{
    public List<string> Check(PoolMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolMilestoneTitleLengthRule(int MaxLength) : IPoolMilestoneRule
{
    public List<string> Check(PoolMilestone item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolMilestoneCodeLengthRule(int MaxLength) : IPoolMilestoneRule
{
    public List<string> Check(PoolMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool milestone.</summary>
public sealed record PoolMilestoneValidator(List<IPoolMilestoneRule> Rules)
{
    public List<string> Validate(PoolMilestone item)
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

    public void RequireValid(PoolMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolMilestone> items)
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

/// <summary>Functions of pool milestone rules.</summary>
public static class PoolMilestoneRules
{
    public static PoolMilestoneValidator DefaultPoolMilestoneValidator()
    {
        return new PoolMilestoneValidator(
            new List<IPoolMilestoneRule>
            {
                new PoolMilestoneRequiredNotesRule("pool milestone needs this field"),
                new PoolMilestoneRequiredCodeRule("pool milestone needs an entry"),
                new PoolMilestoneTitleLengthRule(3600),
                new PoolMilestoneCodeLengthRule(1000),
            });
    }
}
