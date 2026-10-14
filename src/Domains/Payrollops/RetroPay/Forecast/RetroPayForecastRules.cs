namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a retro pay forecast must pass. Check returns its problems, or none.</summary>
public interface IRetroPayForecastRule
{
    List<string> Check(RetroPayForecast item);
}

public sealed record RetroPayForecastDescriptionLengthRule(int MaxLength) : IRetroPayForecastRule
{
    public List<string> Check(RetroPayForecast item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayForecastRequiredRegionRule(string Message) : IRetroPayForecastRule
{
    public List<string> Check(RetroPayForecast item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayForecastCeilingRangeRule(double Min, double Max) : IRetroPayForecastRule
{
    public List<string> Check(RetroPayForecast item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RetroPayForecastAllowedTitleRule(List<string> Allowed) : IRetroPayForecastRule
{
    public List<string> Check(RetroPayForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayForecastQuantityRangeRule(int Min, int Max) : IRetroPayForecastRule
{
    public List<string> Check(RetroPayForecast item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay forecast.</summary>
public sealed record RetroPayForecastValidator(List<IRetroPayForecastRule> Rules)
{
    public List<string> Validate(RetroPayForecast item)
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

    public void RequireValid(RetroPayForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayForecast> items)
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

/// <summary>Functions of retro pay forecast rules.</summary>
public static class RetroPayForecastRules
{
    public static RetroPayForecastValidator DefaultRetroPayForecastValidator()
    {
        return new RetroPayForecastValidator(
            new List<IRetroPayForecastRule>
            {
                new RetroPayForecastDescriptionLengthRule(80),
                new RetroPayForecastRequiredRegionRule("retro pay forecast needs an entry"),
                new RetroPayForecastCeilingRangeRule(5.4, 1810.3),
                new RetroPayForecastAllowedTitleRule(new List<string> { "global", "fallback", "batched" }),
                new RetroPayForecastQuantityRangeRule(32, 255),
            });
    }
}
