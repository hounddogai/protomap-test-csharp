namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>A rule a regional access badge threshold must pass. Check returns its problems, or none.</summary>
public interface IRegionalAccessBadgeThresholdRule
{
    List<string> Check(RegionalAccessBadgeThreshold item);
}

public sealed record RegionalAccessBadgeThresholdRequiredCurrencyRule(string Message) : IRegionalAccessBadgeThresholdRule
{
    public List<string> Check(RegionalAccessBadgeThreshold item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalAccessBadgeThresholdNameLengthRule(int MaxLength) : IRegionalAccessBadgeThresholdRule
{
    public List<string> Check(RegionalAccessBadgeThreshold item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAccessBadgeThresholdCurrencyLengthRule(int MaxLength) : IRegionalAccessBadgeThresholdRule
{
    public List<string> Check(RegionalAccessBadgeThreshold item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAccessBadgeThresholdRequiredCodeRule(string Message) : IRegionalAccessBadgeThresholdRule
{
    public List<string> Check(RegionalAccessBadgeThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional access badge threshold.</summary>
public sealed record RegionalAccessBadgeThresholdValidator(List<IRegionalAccessBadgeThresholdRule> Rules)
{
    public List<string> Validate(RegionalAccessBadgeThreshold item)
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

    public void RequireValid(RegionalAccessBadgeThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalAccessBadgeThreshold> items)
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

/// <summary>Functions of regional access badge threshold rules.</summary>
public static class RegionalAccessBadgeThresholdRules
{
    public static RegionalAccessBadgeThresholdValidator DefaultRegionalAccessBadgeThresholdValidator()
    {
        return new RegionalAccessBadgeThresholdValidator(
            new List<IRegionalAccessBadgeThresholdRule>
            {
                new RegionalAccessBadgeThresholdRequiredCurrencyRule(
                    "regional access badge threshold needs this field"),
                new RegionalAccessBadgeThresholdNameLengthRule(86400),
                new RegionalAccessBadgeThresholdCurrencyLengthRule(120),
                new RegionalAccessBadgeThresholdRequiredCodeRule("regional access badge threshold needs this field"),
            });
    }
}
