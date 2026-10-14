namespace Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>A rule a capped rate metric must pass. Check returns its problems, or none.</summary>
public interface ICappedRateMetricRule
{
    List<string> Check(CappedRateMetric item);
}

public sealed record CappedRateMetricNameLengthRule(int MaxLength) : ICappedRateMetricRule
{
    public List<string> Check(CappedRateMetric item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedRateMetricRequiredNotesRule(string Message) : ICappedRateMetricRule
{
    public List<string> Check(CappedRateMetric item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedRateMetricCodePrefixRule(string Prefix) : ICappedRateMetricRule
{
    public List<string> Check(CappedRateMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedRateMetricDurationMinutesRangeRule(int Min, int Max) : ICappedRateMetricRule
{
    public List<string> Check(CappedRateMetric item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedRateMetricGraceDaysRangeRule(int Min, int Max) : ICappedRateMetricRule
{
    public List<string> Check(CappedRateMetric item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped rate metric.</summary>
public sealed record CappedRateMetricValidator(List<ICappedRateMetricRule> Rules)
{
    public List<string> Validate(CappedRateMetric item)
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

    public void RequireValid(CappedRateMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedRateMetric> items)
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

/// <summary>Functions of capped rate metric rules.</summary>
public static class CappedRateMetricRules
{
    public static CappedRateMetricValidator DefaultCappedRateMetricValidator()
    {
        return new CappedRateMetricValidator(
            new List<ICappedRateMetricRule>
            {
                new CappedRateMetricNameLengthRule(255),
                new CappedRateMetricRequiredNotesRule("capped rate metric needs this field"),
                new CappedRateMetricCodePrefixRule("REF"),
                new CappedRateMetricDurationMinutesRangeRule(16, 64),
                new CappedRateMetricGraceDaysRangeRule(64, 86400),
            });
    }
}
