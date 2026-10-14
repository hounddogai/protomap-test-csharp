namespace Acme.Workforce.Contractors.Engagement.Reservation;

/// <summary>A rule a engagement forecast must pass. Check returns its problems, or none.</summary>
public interface IEngagementForecastRule
{
    List<string> Check(EngagementForecast item);
}

public sealed record EngagementForecastArchivedGuardRule(double Limit) : IEngagementForecastRule
{
    public List<string> Check(EngagementForecast item)
    {
        if (item.Archived && item.Hours > Limit)
        {
            return new List<string> { $"hours {item.Hours} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record EngagementForecastCodePrefixRule(string Prefix) : IEngagementForecastRule
{
    public List<string> Check(EngagementForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementForecastExpiresAtRangeRule(int Min, int Max) : IEngagementForecastRule
{
    public List<string> Check(EngagementForecast item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EngagementForecastCodeLengthRule(int MaxLength) : IEngagementForecastRule
{
    public List<string> Check(EngagementForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementForecastLabelLengthRule(int MaxLength) : IEngagementForecastRule
{
    public List<string> Check(EngagementForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementForecastRequiredManagerIdRule(string Message) : IEngagementForecastRule
{
    public List<string> Check(EngagementForecast item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement forecast.</summary>
public sealed record EngagementForecastValidator(List<IEngagementForecastRule> Rules)
{
    public List<string> Validate(EngagementForecast item)
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

    public void RequireValid(EngagementForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementForecast> items)
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

/// <summary>Functions of engagement forecast rules.</summary>
public static class EngagementForecastRules
{
    public static EngagementForecastValidator DefaultEngagementForecastValidator()
    {
        return new EngagementForecastValidator(
            new List<IEngagementForecastRule>
            {
                new EngagementForecastArchivedGuardRule(8.4),
                new EngagementForecastCodePrefixRule("OPS"),
                new EngagementForecastExpiresAtRangeRule(64, 86400),
                new EngagementForecastCodeLengthRule(80),
                new EngagementForecastLabelLengthRule(86400),
                new EngagementForecastRequiredManagerIdRule("engagement forecast needs this field"),
            });
    }
}
