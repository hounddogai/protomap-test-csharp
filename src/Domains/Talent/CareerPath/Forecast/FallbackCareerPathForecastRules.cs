namespace Acme.Workforce.Talent.CareerPath.Forecast;

/// <summary>A rule a fallback career path forecast must pass. Check returns its problems, or none.</summary>
public interface IFallbackCareerPathForecastRule
{
    List<string> Check(FallbackCareerPathForecast item);
}

public sealed record FallbackCareerPathForecastLabelLengthRule(int MaxLength) : IFallbackCareerPathForecastRule
{
    public List<string> Check(FallbackCareerPathForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackCareerPathForecastCodePrefixRule(string Prefix) : IFallbackCareerPathForecastRule
{
    public List<string> Check(FallbackCareerPathForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackCareerPathForecastApprovedGuardRule(int Limit) : IFallbackCareerPathForecastRule
{
    public List<string> Check(FallbackCareerPathForecast item)
    {
        if (item.Approved && item.Sequence > Limit)
        {
            return new List<string> { $"sequence {item.Sequence} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback career path forecast.</summary>
public sealed record FallbackCareerPathForecastValidator(List<IFallbackCareerPathForecastRule> Rules)
{
    public List<string> Validate(FallbackCareerPathForecast item)
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

    public void RequireValid(FallbackCareerPathForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackCareerPathForecast> items)
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

/// <summary>Functions of fallback career path forecast rules.</summary>
public static class FallbackCareerPathForecastRules
{
    public static FallbackCareerPathForecastValidator DefaultFallbackCareerPathForecastValidator()
    {
        return new FallbackCareerPathForecastValidator(
            new List<IFallbackCareerPathForecastRule>
            {
                new FallbackCareerPathForecastLabelLengthRule(32),
                new FallbackCareerPathForecastCodePrefixRule("OPS"),
                new FallbackCareerPathForecastApprovedGuardRule(3600),
            });
    }
}
