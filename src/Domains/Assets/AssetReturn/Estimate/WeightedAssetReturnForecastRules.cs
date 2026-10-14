namespace Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>A rule a weighted asset return forecast must pass. Check returns its problems, or none.</summary>
public interface IWeightedAssetReturnForecastRule
{
    List<string> Check(WeightedAssetReturnForecast item);
}

public sealed record WeightedAssetReturnForecastCostRangeRule(double Min, double Max) : IWeightedAssetReturnForecastRule
{
    public List<string> Check(WeightedAssetReturnForecast item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedAssetReturnForecastUnitLengthRule(int MaxLength) : IWeightedAssetReturnForecastRule
{
    public List<string> Check(WeightedAssetReturnForecast item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAssetReturnForecastRequiredCountryRule(string Message) : IWeightedAssetReturnForecastRule
{
    public List<string> Check(WeightedAssetReturnForecast item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAssetReturnForecastApprovedGuardRule(int Limit) : IWeightedAssetReturnForecastRule
{
    public List<string> Check(WeightedAssetReturnForecast item)
    {
        if (item.Approved && item.Capacity > Limit)
        {
            return new List<string> { $"capacity {item.Capacity} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAssetReturnForecastNameLengthRule(int MaxLength) : IWeightedAssetReturnForecastRule
{
    public List<string> Check(WeightedAssetReturnForecast item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted asset return forecast.</summary>
public sealed record WeightedAssetReturnForecastValidator(List<IWeightedAssetReturnForecastRule> Rules)
{
    public List<string> Validate(WeightedAssetReturnForecast item)
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

    public void RequireValid(WeightedAssetReturnForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAssetReturnForecast> items)
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

/// <summary>Functions of weighted asset return forecast rules.</summary>
public static class WeightedAssetReturnForecastRules
{
    public static WeightedAssetReturnForecastValidator DefaultWeightedAssetReturnForecastValidator()
    {
        return new WeightedAssetReturnForecastValidator(
            new List<IWeightedAssetReturnForecastRule>
            {
                new WeightedAssetReturnForecastCostRangeRule(7.5, 2812.7),
                new WeightedAssetReturnForecastUnitLengthRule(16),
                new WeightedAssetReturnForecastRequiredCountryRule("weighted asset return forecast needs a value"),
                new WeightedAssetReturnForecastApprovedGuardRule(120),
                new WeightedAssetReturnForecastNameLengthRule(120),
            });
    }
}
