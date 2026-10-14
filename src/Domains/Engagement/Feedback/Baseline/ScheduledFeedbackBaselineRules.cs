namespace Acme.Workforce.Engagement.Feedback.Baseline;

/// <summary>A rule a scheduled feedback baseline must pass. Check returns its problems, or none.</summary>
public interface IScheduledFeedbackBaselineRule
{
    List<string> Check(ScheduledFeedbackBaseline item);
}

public sealed record ScheduledFeedbackBaselineCurrencyLengthRule(int MaxLength) : IScheduledFeedbackBaselineRule
{
    public List<string> Check(ScheduledFeedbackBaseline item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFeedbackBaselineCodePrefixRule(string Prefix) : IScheduledFeedbackBaselineRule
{
    public List<string> Check(ScheduledFeedbackBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFeedbackBaselineCodeLengthRule(int MaxLength) : IScheduledFeedbackBaselineRule
{
    public List<string> Check(ScheduledFeedbackBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFeedbackBaselineRequiredCodeRule(string Message) : IScheduledFeedbackBaselineRule
{
    public List<string> Check(ScheduledFeedbackBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFeedbackBaselineCeilingRangeRule(double Min, double Max) : IScheduledFeedbackBaselineRule
{
    public List<string> Check(ScheduledFeedbackBaseline item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a scheduled feedback baseline.</summary>
public sealed record ScheduledFeedbackBaselineValidator(List<IScheduledFeedbackBaselineRule> Rules)
{
    public List<string> Validate(ScheduledFeedbackBaseline item)
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

    public void RequireValid(ScheduledFeedbackBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledFeedbackBaseline> items)
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

/// <summary>Functions of scheduled feedback baseline rules.</summary>
public static class ScheduledFeedbackBaselineRules
{
    public static ScheduledFeedbackBaselineValidator DefaultScheduledFeedbackBaselineValidator()
    {
        return new ScheduledFeedbackBaselineValidator(
            new List<IScheduledFeedbackBaselineRule>
            {
                new ScheduledFeedbackBaselineCurrencyLengthRule(255),
                new ScheduledFeedbackBaselineCodePrefixRule("ACM"),
                new ScheduledFeedbackBaselineCodeLengthRule(86400),
                new ScheduledFeedbackBaselineRequiredCodeRule("scheduled feedback baseline needs a value"),
                new ScheduledFeedbackBaselineCeilingRangeRule(6.6, 2697.2),
            });
    }
}
