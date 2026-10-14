namespace Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>A rule a mobility forecast must pass. Check returns its problems, or none.</summary>
public interface IMobilityForecastRule
{
    List<string> Check(MobilityForecast item);
}

public sealed record MobilityForecastCurrencyLengthRule(int MaxLength) : IMobilityForecastRule
{
    public List<string> Check(MobilityForecast item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityForecastRequiredNameRule(string Message) : IMobilityForecastRule
{
    public List<string> Check(MobilityForecast item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityForecastWeightRangeRule(double Min, double Max) : IMobilityForecastRule
{
    public List<string> Check(MobilityForecast item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityForecastAmountRangeRule(double Min, double Max) : IMobilityForecastRule
{
    public List<string> Check(MobilityForecast item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityForecastQuantityRangeRule(int Min, int Max) : IMobilityForecastRule
{
    public List<string> Check(MobilityForecast item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility forecast.</summary>
public sealed record MobilityForecastValidator(List<IMobilityForecastRule> Rules)
{
    public List<string> Validate(MobilityForecast item)
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

    public void RequireValid(MobilityForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityForecast> items)
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

/// <summary>Functions of mobility forecast rules.</summary>
public static class MobilityForecastRules
{
    public static MobilityForecastValidator DefaultMobilityForecastValidator()
    {
        return new MobilityForecastValidator(
            new List<IMobilityForecastRule>
            {
                new MobilityForecastCurrencyLengthRule(80),
                new MobilityForecastRequiredNameRule("mobility forecast needs a value"),
                new MobilityForecastWeightRangeRule(4.8, 718.0),
                new MobilityForecastAmountRangeRule(4.0, 4464.7),
                new MobilityForecastQuantityRangeRule(64, 86400),
            });
    }
}
