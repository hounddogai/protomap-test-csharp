namespace Acme.Workforce.Contractors.Rate.RateBatch;

/// <summary>A rule a rate forecast must pass. Check returns its problems, or none.</summary>
public interface IRateForecastRule
{
    List<string> Check(RateForecast item);
}

public sealed record RateForecastRequiredTitleRule(string Message) : IRateForecastRule
{
    public List<string> Check(RateForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateForecastRequiredCategoryRule(string Message) : IRateForecastRule
{
    public List<string> Check(RateForecast item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateForecastCostRangeRule(double Min, double Max) : IRateForecastRule
{
    public List<string> Check(RateForecast item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RateForecastRequiredCodeRule(string Message) : IRateForecastRule
{
    public List<string> Check(RateForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateForecastTitleLengthRule(int MaxLength) : IRateForecastRule
{
    public List<string> Check(RateForecast item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate forecast.</summary>
public sealed record RateForecastValidator(List<IRateForecastRule> Rules)
{
    public List<string> Validate(RateForecast item)
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

    public void RequireValid(RateForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateForecast> items)
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

/// <summary>Functions of rate forecast rules.</summary>
public static class RateForecastRules
{
    public static RateForecastValidator DefaultRateForecastValidator()
    {
        return new RateForecastValidator(
            new List<IRateForecastRule>
            {
                new RateForecastRequiredTitleRule("rate forecast needs a value"),
                new RateForecastRequiredCategoryRule("rate forecast needs this field"),
                new RateForecastCostRangeRule(3.1, 134.1),
                new RateForecastRequiredCodeRule("rate forecast needs a value"),
                new RateForecastTitleLengthRule(1000),
            });
    }
}
