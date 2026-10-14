namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>A rule a tiered retro pay forecast must pass. Check returns its problems, or none.</summary>
public interface ITieredRetroPayForecastRule
{
    List<string> Check(TieredRetroPayForecast item);
}

public sealed record TieredRetroPayForecastNameLengthRule(int MaxLength) : ITieredRetroPayForecastRule
{
    public List<string> Check(TieredRetroPayForecast item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredRetroPayForecastActiveGuardRule(int Limit) : ITieredRetroPayForecastRule
{
    public List<string> Check(TieredRetroPayForecast item)
    {
        if (item.Active && item.GraceDays > Limit)
        {
            return new List<string> { $"grace days {item.GraceDays} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record TieredRetroPayForecastRateRangeRule(double Min, double Max) : ITieredRetroPayForecastRule
{
    public List<string> Check(TieredRetroPayForecast item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tiered retro pay forecast.</summary>
public sealed record TieredRetroPayForecastValidator(List<ITieredRetroPayForecastRule> Rules)
{
    public List<string> Validate(TieredRetroPayForecast item)
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

    public void RequireValid(TieredRetroPayForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredRetroPayForecast> items)
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

/// <summary>Functions of tiered retro pay forecast rules.</summary>
public static class TieredRetroPayForecastRules
{
    public static TieredRetroPayForecastValidator DefaultTieredRetroPayForecastValidator()
    {
        return new TieredRetroPayForecastValidator(
            new List<ITieredRetroPayForecastRule>
            {
                new TieredRetroPayForecastNameLengthRule(86400),
                new TieredRetroPayForecastActiveGuardRule(64),
                new TieredRetroPayForecastRateRangeRule(3.0, 4404.8),
            });
    }
}
