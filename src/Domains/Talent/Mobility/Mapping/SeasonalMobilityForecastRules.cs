namespace Acme.Workforce.Talent.Mobility.Mapping;

/// <summary>A rule a seasonal mobility forecast must pass. Check returns its problems, or none.</summary>
public interface ISeasonalMobilityForecastRule
{
    List<string> Check(SeasonalMobilityForecast item);
}

public sealed record SeasonalMobilityForecastRequiredCodeRule(string Message) : ISeasonalMobilityForecastRule
{
    public List<string> Check(SeasonalMobilityForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMobilityForecastPriorityRangeRule(int Min, int Max) : ISeasonalMobilityForecastRule
{
    public List<string> Check(SeasonalMobilityForecast item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalMobilityForecastCodeLengthRule(int MaxLength) : ISeasonalMobilityForecastRule
{
    public List<string> Check(SeasonalMobilityForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMobilityForecastRatioRangeRule(double Min, double Max) : ISeasonalMobilityForecastRule
{
    public List<string> Check(SeasonalMobilityForecast item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMobilityForecastExternalRefLengthRule(int MaxLength) : ISeasonalMobilityForecastRule
{
    public List<string> Check(SeasonalMobilityForecast item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal mobility forecast.</summary>
public sealed record SeasonalMobilityForecastValidator(List<ISeasonalMobilityForecastRule> Rules)
{
    public List<string> Validate(SeasonalMobilityForecast item)
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

    public void RequireValid(SeasonalMobilityForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalMobilityForecast> items)
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

/// <summary>Functions of seasonal mobility forecast rules.</summary>
public static class SeasonalMobilityForecastRules
{
    public static SeasonalMobilityForecastValidator DefaultSeasonalMobilityForecastValidator()
    {
        return new SeasonalMobilityForecastValidator(
            new List<ISeasonalMobilityForecastRule>
            {
                new SeasonalMobilityForecastRequiredCodeRule("seasonal mobility forecast needs an entry"),
                new SeasonalMobilityForecastPriorityRangeRule(120, 255),
                new SeasonalMobilityForecastCodeLengthRule(32),
                new SeasonalMobilityForecastRatioRangeRule(3.1, 1931.1),
                new SeasonalMobilityForecastExternalRefLengthRule(16),
            });
    }
}
