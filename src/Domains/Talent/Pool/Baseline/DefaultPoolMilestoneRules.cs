namespace Acme.Workforce.Talent.Pool.Baseline;

/// <summary>A rule a default pool milestone must pass. Check returns its problems, or none.</summary>
public interface IDefaultPoolMilestoneRule
{
    List<string> Check(DefaultPoolMilestone item);
}

public sealed record DefaultPoolMilestoneNameLengthRule(int MaxLength) : IDefaultPoolMilestoneRule
{
    public List<string> Check(DefaultPoolMilestone item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPoolMilestoneRequiredCodeRule(string Message) : IDefaultPoolMilestoneRule
{
    public List<string> Check(DefaultPoolMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPoolMilestoneCreatedAtRangeRule(int Min, int Max) : IDefaultPoolMilestoneRule
{
    public List<string> Check(DefaultPoolMilestone item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPoolMilestoneCodeLengthRule(int MaxLength) : IDefaultPoolMilestoneRule
{
    public List<string> Check(DefaultPoolMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default pool milestone.</summary>
public sealed record DefaultPoolMilestoneValidator(List<IDefaultPoolMilestoneRule> Rules)
{
    public List<string> Validate(DefaultPoolMilestone item)
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

    public void RequireValid(DefaultPoolMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPoolMilestone> items)
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

/// <summary>Functions of default pool milestone rules.</summary>
public static class DefaultPoolMilestoneRules
{
    public static DefaultPoolMilestoneValidator DefaultDefaultPoolMilestoneValidator()
    {
        return new DefaultPoolMilestoneValidator(
            new List<IDefaultPoolMilestoneRule>
            {
                new DefaultPoolMilestoneNameLengthRule(80),
                new DefaultPoolMilestoneRequiredCodeRule("default pool milestone needs an entry"),
                new DefaultPoolMilestoneCreatedAtRangeRule(16, 3600),
                new DefaultPoolMilestoneCodeLengthRule(32),
            });
    }
}
