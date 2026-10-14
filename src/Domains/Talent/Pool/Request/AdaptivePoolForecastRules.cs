namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive pool forecast must pass. Check returns its problems, or none.</summary>
public interface IAdaptivePoolForecastRule
{
    List<string> Check(AdaptivePoolForecast item);
}

public sealed record AdaptivePoolForecastRequiredManagerIdRule(string Message) : IAdaptivePoolForecastRule
{
    public List<string> Check(AdaptivePoolForecast item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePoolForecastAllowedTitleRule(List<string> Allowed) : IAdaptivePoolForecastRule
{
    public List<string> Check(AdaptivePoolForecast item)
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

public sealed record AdaptivePoolForecastRequiredTitleRule(string Message) : IAdaptivePoolForecastRule
{
    public List<string> Check(AdaptivePoolForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePoolForecastRequiredCodeRule(string Message) : IAdaptivePoolForecastRule
{
    public List<string> Check(AdaptivePoolForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePoolForecastRequiredLabelRule(string Message) : IAdaptivePoolForecastRule
{
    public List<string> Check(AdaptivePoolForecast item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive pool forecast.</summary>
public sealed record AdaptivePoolForecastValidator(List<IAdaptivePoolForecastRule> Rules)
{
    public List<string> Validate(AdaptivePoolForecast item)
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

    public void RequireValid(AdaptivePoolForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptivePoolForecast> items)
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

/// <summary>Functions of adaptive pool forecast rules.</summary>
public static class AdaptivePoolForecastRules
{
    public static AdaptivePoolForecastValidator DefaultAdaptivePoolForecastValidator()
    {
        return new AdaptivePoolForecastValidator(
            new List<IAdaptivePoolForecastRule>
            {
                new AdaptivePoolForecastRequiredManagerIdRule("adaptive pool forecast needs this field"),
                new AdaptivePoolForecastAllowedTitleRule(new List<string>()),
                new AdaptivePoolForecastRequiredTitleRule("adaptive pool forecast needs an entry"),
                new AdaptivePoolForecastRequiredCodeRule("adaptive pool forecast needs this field"),
                new AdaptivePoolForecastRequiredLabelRule("adaptive pool forecast needs a value"),
            });
    }
}
