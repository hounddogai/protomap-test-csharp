namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict bonus pool group must pass. Check returns its problems, or none.</summary>
public interface IStrictBonusPoolGroupRule
{
    List<string> Check(StrictBonusPoolGroup item);
}

public sealed record StrictBonusPoolGroupCodeLengthRule(int MaxLength) : IStrictBonusPoolGroupRule
{
    public List<string> Check(StrictBonusPoolGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictBonusPoolGroupAllowedChannelRule(List<string> Allowed) : IStrictBonusPoolGroupRule
{
    public List<string> Check(StrictBonusPoolGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record StrictBonusPoolGroupEmployeeIdLengthRule(int MaxLength) : IStrictBonusPoolGroupRule
{
    public List<string> Check(StrictBonusPoolGroup item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict bonus pool group.</summary>
public sealed record StrictBonusPoolGroupValidator(List<IStrictBonusPoolGroupRule> Rules)
{
    public List<string> Validate(StrictBonusPoolGroup item)
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

    public void RequireValid(StrictBonusPoolGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictBonusPoolGroup> items)
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

/// <summary>Functions of strict bonus pool group rules.</summary>
public static class StrictBonusPoolGroupRules
{
    public static StrictBonusPoolGroupValidator DefaultStrictBonusPoolGroupValidator()
    {
        return new StrictBonusPoolGroupValidator(
            new List<IStrictBonusPoolGroupRule>
            {
                new StrictBonusPoolGroupCodeLengthRule(16),
                new StrictBonusPoolGroupAllowedChannelRule(new List<string> { "rolling" }),
                new StrictBonusPoolGroupEmployeeIdLengthRule(1000),
            });
    }
}
