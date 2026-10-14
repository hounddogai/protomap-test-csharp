namespace Acme.Workforce.Compensation.Adjustment.Forecast;

/// <summary>A rule a adjustment forecast must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentForecastRule
{
    List<string> Check(AdjustmentForecast item);
}

public sealed record AdjustmentForecastRateRangeRule(double Min, double Max) : IAdjustmentForecastRule
{
    public List<string> Check(AdjustmentForecast item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentForecastRequiredSalaryBandRule(string Message) : IAdjustmentForecastRule
{
    public List<string> Check(AdjustmentForecast item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentForecastStartsAtRangeRule(int Min, int Max) : IAdjustmentForecastRule
{
    public List<string> Check(AdjustmentForecast item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentForecastCodeLengthRule(int MaxLength) : IAdjustmentForecastRule
{
    public List<string> Check(AdjustmentForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment forecast.</summary>
public sealed record AdjustmentForecastValidator(List<IAdjustmentForecastRule> Rules)
{
    public List<string> Validate(AdjustmentForecast item)
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

    public void RequireValid(AdjustmentForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentForecast> items)
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

/// <summary>Functions of adjustment forecast rules.</summary>
public static class AdjustmentForecastRules
{
    public static AdjustmentForecastValidator DefaultAdjustmentForecastValidator()
    {
        return new AdjustmentForecastValidator(
            new List<IAdjustmentForecastRule>
            {
                new AdjustmentForecastRateRangeRule(7.6, 2428.4),
                new AdjustmentForecastRequiredSalaryBandRule("adjustment forecast needs a value"),
                new AdjustmentForecastStartsAtRangeRule(120, 3600),
                new AdjustmentForecastCodeLengthRule(86400),
            });
    }
}
