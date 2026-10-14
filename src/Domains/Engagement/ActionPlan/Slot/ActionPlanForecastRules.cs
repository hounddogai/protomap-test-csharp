namespace Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>A rule a action plan forecast must pass. Check returns its problems, or none.</summary>
public interface IActionPlanForecastRule
{
    List<string> Check(ActionPlanForecast item);
}

public sealed record ActionPlanForecastVersionRangeRule(int Min, int Max) : IActionPlanForecastRule
{
    public List<string> Check(ActionPlanForecast item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanForecastCodePrefixRule(string Prefix) : IActionPlanForecastRule
{
    public List<string> Check(ActionPlanForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanForecastRateRangeRule(double Min, double Max) : IActionPlanForecastRule
{
    public List<string> Check(ActionPlanForecast item)
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

public sealed record ActionPlanForecastPhoneLengthRule(int MaxLength) : IActionPlanForecastRule
{
    public List<string> Check(ActionPlanForecast item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan forecast.</summary>
public sealed record ActionPlanForecastValidator(List<IActionPlanForecastRule> Rules)
{
    public List<string> Validate(ActionPlanForecast item)
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

    public void RequireValid(ActionPlanForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanForecast> items)
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

/// <summary>Functions of action plan forecast rules.</summary>
public static class ActionPlanForecastRules
{
    public static ActionPlanForecastValidator DefaultActionPlanForecastValidator()
    {
        return new ActionPlanForecastValidator(
            new List<IActionPlanForecastRule>
            {
                new ActionPlanForecastVersionRangeRule(255, 86400),
                new ActionPlanForecastCodePrefixRule("ACM"),
                new ActionPlanForecastRateRangeRule(6.9, 1519.3),
                new ActionPlanForecastPhoneLengthRule(16),
            });
    }
}
