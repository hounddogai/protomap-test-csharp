namespace Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>A rule a default bonus pool allocation must pass. Check returns its problems, or none.</summary>
public interface IDefaultBonusPoolAllocationRule
{
    List<string> Check(DefaultBonusPoolAllocation item);
}

public sealed record DefaultBonusPoolAllocationRequiredNameRule(string Message) : IDefaultBonusPoolAllocationRule
{
    public List<string> Check(DefaultBonusPoolAllocation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultBonusPoolAllocationCodeLengthRule(int MaxLength) : IDefaultBonusPoolAllocationRule
{
    public List<string> Check(DefaultBonusPoolAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBonusPoolAllocationRequiredDescriptionRule(string Message) : IDefaultBonusPoolAllocationRule
{
    public List<string> Check(DefaultBonusPoolAllocation item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultBonusPoolAllocationMaximumRangeRule(double Min, double Max) : IDefaultBonusPoolAllocationRule
{
    public List<string> Check(DefaultBonusPoolAllocation item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultBonusPoolAllocationTeamLengthRule(int MaxLength) : IDefaultBonusPoolAllocationRule
{
    public List<string> Check(DefaultBonusPoolAllocation item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBonusPoolAllocationRequiredCodeRule(string Message) : IDefaultBonusPoolAllocationRule
{
    public List<string> Check(DefaultBonusPoolAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default bonus pool allocation.</summary>
public sealed record DefaultBonusPoolAllocationValidator(List<IDefaultBonusPoolAllocationRule> Rules)
{
    public List<string> Validate(DefaultBonusPoolAllocation item)
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

    public void RequireValid(DefaultBonusPoolAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultBonusPoolAllocation> items)
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

/// <summary>Functions of default bonus pool allocation rules.</summary>
public static class DefaultBonusPoolAllocationRules
{
    public static DefaultBonusPoolAllocationValidator DefaultDefaultBonusPoolAllocationValidator()
    {
        return new DefaultBonusPoolAllocationValidator(
            new List<IDefaultBonusPoolAllocationRule>
            {
                new DefaultBonusPoolAllocationRequiredNameRule("default bonus pool allocation needs an entry"),
                new DefaultBonusPoolAllocationCodeLengthRule(32),
                new DefaultBonusPoolAllocationRequiredDescriptionRule("default bonus pool allocation needs a value"),
                new DefaultBonusPoolAllocationMaximumRangeRule(5.1, 1925.2),
                new DefaultBonusPoolAllocationTeamLengthRule(16),
                new DefaultBonusPoolAllocationRequiredCodeRule("default bonus pool allocation needs a value"),
            });
    }
}
