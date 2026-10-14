namespace Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>A rule a capped retro pay forecast must pass. Check returns its problems, or none.</summary>
public interface ICappedRetroPayForecastRule
{
    List<string> Check(CappedRetroPayForecast item);
}

public sealed record CappedRetroPayForecastExpiresAtRangeRule(int Min, int Max) : ICappedRetroPayForecastRule
{
    public List<string> Check(CappedRetroPayForecast item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedRetroPayForecastCodeLengthRule(int MaxLength) : ICappedRetroPayForecastRule
{
    public List<string> Check(CappedRetroPayForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedRetroPayForecastRequiredCodeRule(string Message) : ICappedRetroPayForecastRule
{
    public List<string> Check(CappedRetroPayForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped retro pay forecast.</summary>
public sealed record CappedRetroPayForecastValidator(List<ICappedRetroPayForecastRule> Rules)
{
    public List<string> Validate(CappedRetroPayForecast item)
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

    public void RequireValid(CappedRetroPayForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedRetroPayForecast> items)
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

/// <summary>Functions of capped retro pay forecast rules.</summary>
public static class CappedRetroPayForecastRules
{
    public static CappedRetroPayForecastValidator DefaultCappedRetroPayForecastValidator()
    {
        return new CappedRetroPayForecastValidator(
            new List<ICappedRetroPayForecastRule>
            {
                new CappedRetroPayForecastExpiresAtRangeRule(32, 120),
                new CappedRetroPayForecastCodeLengthRule(120),
                new CappedRetroPayForecastRequiredCodeRule("capped retro pay forecast needs an entry"),
            });
    }
}
