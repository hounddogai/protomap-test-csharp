namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey summary must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveySummaryRule
{
    List<string> Check(PulseSurveySummary item);
}

public sealed record PulseSurveySummaryPinnedGuardRule(int Limit) : IPulseSurveySummaryRule
{
    public List<string> Check(PulseSurveySummary item)
    {
        if (item.Pinned && item.Capacity > Limit)
        {
            return new List<string> { $"capacity {item.Capacity} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveySummaryThresholdRangeRule(double Min, double Max) : IPulseSurveySummaryRule
{
    public List<string> Check(PulseSurveySummary item)
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

public sealed record PulseSurveySummaryCurrencyLengthRule(int MaxLength) : IPulseSurveySummaryRule
{
    public List<string> Check(PulseSurveySummary item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveySummaryAllowedCurrencyRule(List<string> Allowed) : IPulseSurveySummaryRule
{
    public List<string> Check(PulseSurveySummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey summary.</summary>
public sealed record PulseSurveySummaryValidator(List<IPulseSurveySummaryRule> Rules)
{
    public List<string> Validate(PulseSurveySummary item)
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

    public void RequireValid(PulseSurveySummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveySummary> items)
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

/// <summary>Functions of pulse survey summary rules.</summary>
public static class PulseSurveySummaryRules
{
    public static PulseSurveySummaryValidator DefaultPulseSurveySummaryValidator()
    {
        return new PulseSurveySummaryValidator(
            new List<IPulseSurveySummaryRule>
            {
                new PulseSurveySummaryPinnedGuardRule(16),
                new PulseSurveySummaryThresholdRangeRule(8.5, 2088.2),
                new PulseSurveySummaryCurrencyLengthRule(64),
                new PulseSurveySummaryAllowedCurrencyRule(new List<string> { "capped" }),
            });
    }
}
