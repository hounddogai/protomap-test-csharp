namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

/// <summary>A rule a strict adjustment baseline must pass. Check returns its problems, or none.</summary>
public interface IStrictAdjustmentBaselineRule
{
    List<string> Check(StrictAdjustmentBaseline item);
}

public sealed record StrictAdjustmentBaselineCodeLengthRule(int MaxLength) : IStrictAdjustmentBaselineRule
{
    public List<string> Check(StrictAdjustmentBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAdjustmentBaselineNameLengthRule(int MaxLength) : IStrictAdjustmentBaselineRule
{
    public List<string> Check(StrictAdjustmentBaseline item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAdjustmentBaselineGraceDaysRangeRule(int Min, int Max) : IStrictAdjustmentBaselineRule
{
    public List<string> Check(StrictAdjustmentBaseline item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAdjustmentBaselineSkillLengthRule(int MaxLength) : IStrictAdjustmentBaselineRule
{
    public List<string> Check(StrictAdjustmentBaseline item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAdjustmentBaselineRequiredNameRule(string Message) : IStrictAdjustmentBaselineRule
{
    public List<string> Check(StrictAdjustmentBaseline item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAdjustmentBaselineCodePrefixRule(string Prefix) : IStrictAdjustmentBaselineRule
{
    public List<string> Check(StrictAdjustmentBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict adjustment baseline.</summary>
public sealed record StrictAdjustmentBaselineValidator(List<IStrictAdjustmentBaselineRule> Rules)
{
    public List<string> Validate(StrictAdjustmentBaseline item)
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

    public void RequireValid(StrictAdjustmentBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAdjustmentBaseline> items)
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

/// <summary>Functions of strict adjustment baseline rules.</summary>
public static class StrictAdjustmentBaselineRules
{
    public static StrictAdjustmentBaselineValidator DefaultStrictAdjustmentBaselineValidator()
    {
        return new StrictAdjustmentBaselineValidator(
            new List<IStrictAdjustmentBaselineRule>
            {
                new StrictAdjustmentBaselineCodeLengthRule(80),
                new StrictAdjustmentBaselineNameLengthRule(32),
                new StrictAdjustmentBaselineGraceDaysRangeRule(32, 80),
                new StrictAdjustmentBaselineSkillLengthRule(16),
                new StrictAdjustmentBaselineRequiredNameRule("strict adjustment baseline needs an entry"),
                new StrictAdjustmentBaselineCodePrefixRule("X"),
            });
    }
}
