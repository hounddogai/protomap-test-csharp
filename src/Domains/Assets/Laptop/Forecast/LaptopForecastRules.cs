namespace Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>A rule a laptop forecast must pass. Check returns its problems, or none.</summary>
public interface ILaptopForecastRule
{
    List<string> Check(LaptopForecast item);
}

public sealed record LaptopForecastLabelLengthRule(int MaxLength) : ILaptopForecastRule
{
    public List<string> Check(LaptopForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopForecastThresholdRangeRule(double Min, double Max) : ILaptopForecastRule
{
    public List<string> Check(LaptopForecast item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopForecastCodeLengthRule(int MaxLength) : ILaptopForecastRule
{
    public List<string> Check(LaptopForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop forecast.</summary>
public sealed record LaptopForecastValidator(List<ILaptopForecastRule> Rules)
{
    public List<string> Validate(LaptopForecast item)
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

    public void RequireValid(LaptopForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopForecast> items)
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

/// <summary>Functions of laptop forecast rules.</summary>
public static class LaptopForecastRules
{
    public static LaptopForecastValidator DefaultLaptopForecastValidator()
    {
        return new LaptopForecastValidator(
            new List<ILaptopForecastRule>
            {
                new LaptopForecastLabelLengthRule(80),
                new LaptopForecastThresholdRangeRule(0.0, 1694.3),
                new LaptopForecastCodeLengthRule(1000),
            });
    }
}
