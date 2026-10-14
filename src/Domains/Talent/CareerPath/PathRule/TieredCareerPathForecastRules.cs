namespace Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>A rule a tiered career path forecast must pass. Check returns its problems, or none.</summary>
public interface ITieredCareerPathForecastRule
{
    List<string> Check(TieredCareerPathForecast item);
}

public sealed record TieredCareerPathForecastDescriptionLengthRule(int MaxLength) : ITieredCareerPathForecastRule
{
    public List<string> Check(TieredCareerPathForecast item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredCareerPathForecastRequiredRegionRule(string Message) : ITieredCareerPathForecastRule
{
    public List<string> Check(TieredCareerPathForecast item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredCareerPathForecastRegionLengthRule(int MaxLength) : ITieredCareerPathForecastRule
{
    public List<string> Check(TieredCareerPathForecast item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredCareerPathForecastApprovedGuardRule(int Limit) : ITieredCareerPathForecastRule
{
    public List<string> Check(TieredCareerPathForecast item)
    {
        if (item.Approved && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record TieredCareerPathForecastAttemptsRangeRule(int Min, int Max) : ITieredCareerPathForecastRule
{
    public List<string> Check(TieredCareerPathForecast item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tiered career path forecast.</summary>
public sealed record TieredCareerPathForecastValidator(List<ITieredCareerPathForecastRule> Rules)
{
    public List<string> Validate(TieredCareerPathForecast item)
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

    public void RequireValid(TieredCareerPathForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredCareerPathForecast> items)
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

/// <summary>Functions of tiered career path forecast rules.</summary>
public static class TieredCareerPathForecastRules
{
    public static TieredCareerPathForecastValidator DefaultTieredCareerPathForecastValidator()
    {
        return new TieredCareerPathForecastValidator(
            new List<ITieredCareerPathForecastRule>
            {
                new TieredCareerPathForecastDescriptionLengthRule(32),
                new TieredCareerPathForecastRequiredRegionRule("tiered career path forecast needs a value"),
                new TieredCareerPathForecastRegionLengthRule(80),
                new TieredCareerPathForecastApprovedGuardRule(80),
                new TieredCareerPathForecastAttemptsRangeRule(255, 3600),
            });
    }
}
