namespace Acme.Workforce.Contractors.Rate.Score;

/// <summary>A rule a strict rate forecast must pass. Check returns its problems, or none.</summary>
public interface IStrictRateForecastRule
{
    List<string> Check(StrictRateForecast item);
}

public sealed record StrictRateForecastMaximumRangeRule(double Min, double Max) : IStrictRateForecastRule
{
    public List<string> Check(StrictRateForecast item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictRateForecastStartsAtRangeRule(int Min, int Max) : IStrictRateForecastRule
{
    public List<string> Check(StrictRateForecast item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictRateForecastCodeLengthRule(int MaxLength) : IStrictRateForecastRule
{
    public List<string> Check(StrictRateForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict rate forecast.</summary>
public sealed record StrictRateForecastValidator(List<IStrictRateForecastRule> Rules)
{
    public List<string> Validate(StrictRateForecast item)
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

    public void RequireValid(StrictRateForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictRateForecast> items)
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

/// <summary>Functions of strict rate forecast rules.</summary>
public static class StrictRateForecastRules
{
    public static StrictRateForecastValidator DefaultStrictRateForecastValidator()
    {
        return new StrictRateForecastValidator(
            new List<IStrictRateForecastRule>
            {
                new StrictRateForecastMaximumRangeRule(7.5, 1479.5),
                new StrictRateForecastStartsAtRangeRule(64, 120),
                new StrictRateForecastCodeLengthRule(3600),
            });
    }
}
