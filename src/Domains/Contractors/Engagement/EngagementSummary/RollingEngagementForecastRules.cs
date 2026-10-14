namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>A rule a rolling engagement forecast must pass. Check returns its problems, or none.</summary>
public interface IRollingEngagementForecastRule
{
    List<string> Check(RollingEngagementForecast item);
}

public sealed record RollingEngagementForecastSummaryLengthRule(int MaxLength) : IRollingEngagementForecastRule
{
    public List<string> Check(RollingEngagementForecast item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingEngagementForecastLocaleLengthRule(int MaxLength) : IRollingEngagementForecastRule
{
    public List<string> Check(RollingEngagementForecast item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingEngagementForecastLabelLengthRule(int MaxLength) : IRollingEngagementForecastRule
{
    public List<string> Check(RollingEngagementForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingEngagementForecastRequiredLabelRule(string Message) : IRollingEngagementForecastRule
{
    public List<string> Check(RollingEngagementForecast item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingEngagementForecastCodePrefixRule(string Prefix) : IRollingEngagementForecastRule
{
    public List<string> Check(RollingEngagementForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingEngagementForecastGraceDaysRangeRule(int Min, int Max) : IRollingEngagementForecastRule
{
    public List<string> Check(RollingEngagementForecast item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling engagement forecast.</summary>
public sealed record RollingEngagementForecastValidator(List<IRollingEngagementForecastRule> Rules)
{
    public List<string> Validate(RollingEngagementForecast item)
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

    public void RequireValid(RollingEngagementForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingEngagementForecast> items)
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

/// <summary>Functions of rolling engagement forecast rules.</summary>
public static class RollingEngagementForecastRules
{
    public static RollingEngagementForecastValidator DefaultRollingEngagementForecastValidator()
    {
        return new RollingEngagementForecastValidator(
            new List<IRollingEngagementForecastRule>
            {
                new RollingEngagementForecastSummaryLengthRule(1000),
                new RollingEngagementForecastLocaleLengthRule(3600),
                new RollingEngagementForecastLabelLengthRule(1000),
                new RollingEngagementForecastRequiredLabelRule("rolling engagement forecast needs this field"),
                new RollingEngagementForecastCodePrefixRule("OPS"),
                new RollingEngagementForecastGraceDaysRangeRule(64, 255),
            });
    }
}
