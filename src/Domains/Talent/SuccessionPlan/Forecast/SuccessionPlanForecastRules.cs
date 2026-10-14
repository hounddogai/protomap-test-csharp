namespace Acme.Workforce.Talent.SuccessionPlan.Forecast;

/// <summary>A rule a succession plan forecast must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanForecastRule
{
    List<string> Check(SuccessionPlanForecast item);
}

public sealed record SuccessionPlanForecastQuantityRangeRule(int Min, int Max) : ISuccessionPlanForecastRule
{
    public List<string> Check(SuccessionPlanForecast item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SuccessionPlanForecastRequiredCodeRule(string Message) : ISuccessionPlanForecastRule
{
    public List<string> Check(SuccessionPlanForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanForecastRevisionRangeRule(int Min, int Max) : ISuccessionPlanForecastRule
{
    public List<string> Check(SuccessionPlanForecast item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanForecastCodePrefixRule(string Prefix) : ISuccessionPlanForecastRule
{
    public List<string> Check(SuccessionPlanForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan forecast.</summary>
public sealed record SuccessionPlanForecastValidator(List<ISuccessionPlanForecastRule> Rules)
{
    public List<string> Validate(SuccessionPlanForecast item)
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

    public void RequireValid(SuccessionPlanForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanForecast> items)
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

/// <summary>Functions of succession plan forecast rules.</summary>
public static class SuccessionPlanForecastRules
{
    public static SuccessionPlanForecastValidator DefaultSuccessionPlanForecastValidator()
    {
        return new SuccessionPlanForecastValidator(
            new List<ISuccessionPlanForecastRule>
            {
                new SuccessionPlanForecastQuantityRangeRule(32, 86400),
                new SuccessionPlanForecastRequiredCodeRule("succession plan forecast needs an entry"),
                new SuccessionPlanForecastRevisionRangeRule(3600, 86400),
                new SuccessionPlanForecastCodePrefixRule("X"),
            });
    }
}
