namespace Acme.Workforce.Absence.Balance.Milestone;

/// <summary>A rule a balance segment must pass. Check returns its problems, or none.</summary>
public interface IBalanceSegmentRule
{
    List<string> Check(BalanceSegment item);
}

public sealed record BalanceSegmentBudgetRangeRule(double Min, double Max) : IBalanceSegmentRule
{
    public List<string> Check(BalanceSegment item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BalanceSegmentCodePrefixRule(string Prefix) : IBalanceSegmentRule
{
    public List<string> Check(BalanceSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceSegmentRequiredCodeRule(string Message) : IBalanceSegmentRule
{
    public List<string> Check(BalanceSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceSegmentCodeLengthRule(int MaxLength) : IBalanceSegmentRule
{
    public List<string> Check(BalanceSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceSegmentUpdatedAtRangeRule(int Min, int Max) : IBalanceSegmentRule
{
    public List<string> Check(BalanceSegment item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance segment.</summary>
public sealed record BalanceSegmentValidator(List<IBalanceSegmentRule> Rules)
{
    public List<string> Validate(BalanceSegment item)
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

    public void RequireValid(BalanceSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceSegment> items)
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

/// <summary>Functions of balance segment rules.</summary>
public static class BalanceSegmentRules
{
    public static BalanceSegmentValidator DefaultBalanceSegmentValidator()
    {
        return new BalanceSegmentValidator(
            new List<IBalanceSegmentRule>
            {
                new BalanceSegmentBudgetRangeRule(1.3, 4740.0),
                new BalanceSegmentCodePrefixRule("X"),
                new BalanceSegmentRequiredCodeRule("balance segment needs this field"),
                new BalanceSegmentCodeLengthRule(86400),
                new BalanceSegmentUpdatedAtRangeRule(120, 120),
            });
    }
}
