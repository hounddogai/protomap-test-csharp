namespace Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>A rule a rolling accrual segment must pass. Check returns its problems, or none.</summary>
public interface IRollingAccrualSegmentRule
{
    List<string> Check(RollingAccrualSegment item);
}

public sealed record RollingAccrualSegmentAmountRangeRule(double Min, double Max) : IRollingAccrualSegmentRule
{
    public List<string> Check(RollingAccrualSegment item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualSegmentCodeLengthRule(int MaxLength) : IRollingAccrualSegmentRule
{
    public List<string> Check(RollingAccrualSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualSegmentLocaleLengthRule(int MaxLength) : IRollingAccrualSegmentRule
{
    public List<string> Check(RollingAccrualSegment item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualSegmentRequiredLocationRule(string Message) : IRollingAccrualSegmentRule
{
    public List<string> Check(RollingAccrualSegment item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualSegmentMaximumRangeRule(double Min, double Max) : IRollingAccrualSegmentRule
{
    public List<string> Check(RollingAccrualSegment item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccrualSegmentCodePrefixRule(string Prefix) : IRollingAccrualSegmentRule
{
    public List<string> Check(RollingAccrualSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling accrual segment.</summary>
public sealed record RollingAccrualSegmentValidator(List<IRollingAccrualSegmentRule> Rules)
{
    public List<string> Validate(RollingAccrualSegment item)
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

    public void RequireValid(RollingAccrualSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingAccrualSegment> items)
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

/// <summary>Functions of rolling accrual segment rules.</summary>
public static class RollingAccrualSegmentRules
{
    public static RollingAccrualSegmentValidator DefaultRollingAccrualSegmentValidator()
    {
        return new RollingAccrualSegmentValidator(
            new List<IRollingAccrualSegmentRule>
            {
                new RollingAccrualSegmentAmountRangeRule(8.0, 4823.8),
                new RollingAccrualSegmentCodeLengthRule(32),
                new RollingAccrualSegmentLocaleLengthRule(120),
                new RollingAccrualSegmentRequiredLocationRule("rolling accrual segment needs an entry"),
                new RollingAccrualSegmentMaximumRangeRule(1.9, 1318.7),
                new RollingAccrualSegmentCodePrefixRule("X"),
            });
    }
}
