namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>A rule a fallback bonus pool milestone must pass. Check returns its problems, or none.</summary>
public interface IFallbackBonusPoolMilestoneRule
{
    List<string> Check(FallbackBonusPoolMilestone item);
}

public sealed record FallbackBonusPoolMilestoneExternalRefLengthRule(int MaxLength) : IFallbackBonusPoolMilestoneRule
{
    public List<string> Check(FallbackBonusPoolMilestone item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBonusPoolMilestoneCodeLengthRule(int MaxLength) : IFallbackBonusPoolMilestoneRule
{
    public List<string> Check(FallbackBonusPoolMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBonusPoolMilestoneRequiredNameRule(string Message) : IFallbackBonusPoolMilestoneRule
{
    public List<string> Check(FallbackBonusPoolMilestone item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackBonusPoolMilestoneCodePrefixRule(string Prefix) : IFallbackBonusPoolMilestoneRule
{
    public List<string> Check(FallbackBonusPoolMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBonusPoolMilestoneCapacityRangeRule(int Min, int Max) : IFallbackBonusPoolMilestoneRule
{
    public List<string> Check(FallbackBonusPoolMilestone item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a fallback bonus pool milestone.</summary>
public sealed record FallbackBonusPoolMilestoneValidator(List<IFallbackBonusPoolMilestoneRule> Rules)
{
    public List<string> Validate(FallbackBonusPoolMilestone item)
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

    public void RequireValid(FallbackBonusPoolMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackBonusPoolMilestone> items)
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

/// <summary>Functions of fallback bonus pool milestone rules.</summary>
public static class FallbackBonusPoolMilestoneRules
{
    public static FallbackBonusPoolMilestoneValidator DefaultFallbackBonusPoolMilestoneValidator()
    {
        return new FallbackBonusPoolMilestoneValidator(
            new List<IFallbackBonusPoolMilestoneRule>
            {
                new FallbackBonusPoolMilestoneExternalRefLengthRule(1000),
                new FallbackBonusPoolMilestoneCodeLengthRule(80),
                new FallbackBonusPoolMilestoneRequiredNameRule("fallback bonus pool milestone needs an entry"),
                new FallbackBonusPoolMilestoneCodePrefixRule("Q"),
                new FallbackBonusPoolMilestoneCapacityRangeRule(16, 255),
            });
    }
}
