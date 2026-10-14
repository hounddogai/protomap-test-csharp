namespace Acme.Workforce.Compensation.EquityGrant.Adjustment;

/// <summary>A rule a equity grant adjustment must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantAdjustmentRule
{
    List<string> Check(EquityGrantAdjustment item);
}

public sealed record EquityGrantAdjustmentActiveGuardRule(double Limit) : IEquityGrantAdjustmentRule
{
    public List<string> Check(EquityGrantAdjustment item)
    {
        if (item.Active && item.Maximum > Limit)
        {
            return new List<string> { $"maximum {item.Maximum} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantAdjustmentTitleLengthRule(int MaxLength) : IEquityGrantAdjustmentRule
{
    public List<string> Check(EquityGrantAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantAdjustmentRequiredGradeRule(string Message) : IEquityGrantAdjustmentRule
{
    public List<string> Check(EquityGrantAdjustment item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantAdjustmentPhoneLengthRule(int MaxLength) : IEquityGrantAdjustmentRule
{
    public List<string> Check(EquityGrantAdjustment item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantAdjustmentRequiredCodeRule(string Message) : IEquityGrantAdjustmentRule
{
    public List<string> Check(EquityGrantAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantAdjustmentCodePrefixRule(string Prefix) : IEquityGrantAdjustmentRule
{
    public List<string> Check(EquityGrantAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant adjustment.</summary>
public sealed record EquityGrantAdjustmentValidator(List<IEquityGrantAdjustmentRule> Rules)
{
    public List<string> Validate(EquityGrantAdjustment item)
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

    public void RequireValid(EquityGrantAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantAdjustment> items)
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

/// <summary>Functions of equity grant adjustment rules.</summary>
public static class EquityGrantAdjustmentRules
{
    public static EquityGrantAdjustmentValidator DefaultEquityGrantAdjustmentValidator()
    {
        return new EquityGrantAdjustmentValidator(
            new List<IEquityGrantAdjustmentRule>
            {
                new EquityGrantAdjustmentActiveGuardRule(4.3),
                new EquityGrantAdjustmentTitleLengthRule(16),
                new EquityGrantAdjustmentRequiredGradeRule("equity grant adjustment needs this field"),
                new EquityGrantAdjustmentPhoneLengthRule(120),
                new EquityGrantAdjustmentRequiredCodeRule("equity grant adjustment needs a value"),
                new EquityGrantAdjustmentCodePrefixRule("X"),
            });
    }
}
