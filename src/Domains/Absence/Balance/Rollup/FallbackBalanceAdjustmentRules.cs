namespace Acme.Workforce.Absence.Balance.Rollup;

/// <summary>A rule a fallback balance adjustment must pass. Check returns its problems, or none.</summary>
public interface IFallbackBalanceAdjustmentRule
{
    List<string> Check(FallbackBalanceAdjustment item);
}

public sealed record FallbackBalanceAdjustmentRequiredGradeRule(string Message) : IFallbackBalanceAdjustmentRule
{
    public List<string> Check(FallbackBalanceAdjustment item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackBalanceAdjustmentCodeLengthRule(int MaxLength) : IFallbackBalanceAdjustmentRule
{
    public List<string> Check(FallbackBalanceAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBalanceAdjustmentLabelLengthRule(int MaxLength) : IFallbackBalanceAdjustmentRule
{
    public List<string> Check(FallbackBalanceAdjustment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBalanceAdjustmentRequiredLabelRule(string Message) : IFallbackBalanceAdjustmentRule
{
    public List<string> Check(FallbackBalanceAdjustment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackBalanceAdjustmentCodePrefixRule(string Prefix) : IFallbackBalanceAdjustmentRule
{
    public List<string> Check(FallbackBalanceAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBalanceAdjustmentGradeLengthRule(int MaxLength) : IFallbackBalanceAdjustmentRule
{
    public List<string> Check(FallbackBalanceAdjustment item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback balance adjustment.</summary>
public sealed record FallbackBalanceAdjustmentValidator(List<IFallbackBalanceAdjustmentRule> Rules)
{
    public List<string> Validate(FallbackBalanceAdjustment item)
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

    public void RequireValid(FallbackBalanceAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackBalanceAdjustment> items)
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

/// <summary>Functions of fallback balance adjustment rules.</summary>
public static class FallbackBalanceAdjustmentRules
{
    public static FallbackBalanceAdjustmentValidator DefaultFallbackBalanceAdjustmentValidator()
    {
        return new FallbackBalanceAdjustmentValidator(
            new List<IFallbackBalanceAdjustmentRule>
            {
                new FallbackBalanceAdjustmentRequiredGradeRule("fallback balance adjustment needs an entry"),
                new FallbackBalanceAdjustmentCodeLengthRule(120),
                new FallbackBalanceAdjustmentLabelLengthRule(32),
                new FallbackBalanceAdjustmentRequiredLabelRule("fallback balance adjustment needs an entry"),
                new FallbackBalanceAdjustmentCodePrefixRule("REF"),
                new FallbackBalanceAdjustmentGradeLengthRule(80),
            });
    }
}
