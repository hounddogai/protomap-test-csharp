namespace Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>A rule a scheduled accrual baseline must pass. Check returns its problems, or none.</summary>
public interface IScheduledAccrualBaselineRule
{
    List<string> Check(ScheduledAccrualBaseline item);
}

public sealed record ScheduledAccrualBaselineCodeLengthRule(int MaxLength) : IScheduledAccrualBaselineRule
{
    public List<string> Check(ScheduledAccrualBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccrualBaselineRequiredTitleRule(string Message) : IScheduledAccrualBaselineRule
{
    public List<string> Check(ScheduledAccrualBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccrualBaselineQuantityRangeRule(int Min, int Max) : IScheduledAccrualBaselineRule
{
    public List<string> Check(ScheduledAccrualBaseline item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledAccrualBaselineThresholdRangeRule(double Min, double Max) : IScheduledAccrualBaselineRule
{
    public List<string> Check(ScheduledAccrualBaseline item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledAccrualBaselineCodePrefixRule(string Prefix) : IScheduledAccrualBaselineRule
{
    public List<string> Check(ScheduledAccrualBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled accrual baseline.</summary>
public sealed record ScheduledAccrualBaselineValidator(List<IScheduledAccrualBaselineRule> Rules)
{
    public List<string> Validate(ScheduledAccrualBaseline item)
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

    public void RequireValid(ScheduledAccrualBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAccrualBaseline> items)
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

/// <summary>Functions of scheduled accrual baseline rules.</summary>
public static class ScheduledAccrualBaselineRules
{
    public static ScheduledAccrualBaselineValidator DefaultScheduledAccrualBaselineValidator()
    {
        return new ScheduledAccrualBaselineValidator(
            new List<IScheduledAccrualBaselineRule>
            {
                new ScheduledAccrualBaselineCodeLengthRule(120),
                new ScheduledAccrualBaselineRequiredTitleRule("scheduled accrual baseline needs an entry"),
                new ScheduledAccrualBaselineQuantityRangeRule(16, 1000),
                new ScheduledAccrualBaselineThresholdRangeRule(1.9, 4926.5),
                new ScheduledAccrualBaselineCodePrefixRule("X"),
            });
    }
}
