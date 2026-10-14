namespace Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>A rule a access badge forecast must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeForecastRule
{
    List<string> Check(AccessBadgeForecast item);
}

public sealed record AccessBadgeForecastCodePrefixRule(string Prefix) : IAccessBadgeForecastRule
{
    public List<string> Check(AccessBadgeForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeForecastRequiredTimezoneRule(string Message) : IAccessBadgeForecastRule
{
    public List<string> Check(AccessBadgeForecast item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeForecastThresholdRangeRule(double Min, double Max) : IAccessBadgeForecastRule
{
    public List<string> Check(AccessBadgeForecast item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccessBadgeForecastManagerIdLengthRule(int MaxLength) : IAccessBadgeForecastRule
{
    public List<string> Check(AccessBadgeForecast item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge forecast.</summary>
public sealed record AccessBadgeForecastValidator(List<IAccessBadgeForecastRule> Rules)
{
    public List<string> Validate(AccessBadgeForecast item)
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

    public void RequireValid(AccessBadgeForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeForecast> items)
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

/// <summary>Functions of access badge forecast rules.</summary>
public static class AccessBadgeForecastRules
{
    public static AccessBadgeForecastValidator DefaultAccessBadgeForecastValidator()
    {
        return new AccessBadgeForecastValidator(
            new List<IAccessBadgeForecastRule>
            {
                new AccessBadgeForecastCodePrefixRule("Q"),
                new AccessBadgeForecastRequiredTimezoneRule("access badge forecast needs an entry"),
                new AccessBadgeForecastThresholdRangeRule(4.2, 1509.8),
                new AccessBadgeForecastManagerIdLengthRule(80),
            });
    }
}
