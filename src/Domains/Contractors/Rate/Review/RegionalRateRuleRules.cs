namespace Acme.Workforce.Contractors.Rate.Review;

/// <summary>A rule a regional rate rule must pass. Check returns its problems, or none.</summary>
public interface IRegionalRateRuleRule
{
    List<string> Check(RegionalRateRule item);
}

public sealed record RegionalRateRuleCodePrefixRule(string Prefix) : IRegionalRateRuleRule
{
    public List<string> Check(RegionalRateRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalRateRuleRequiredCodeRule(string Message) : IRegionalRateRuleRule
{
    public List<string> Check(RegionalRateRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalRateRuleLimitRangeRule(int Min, int Max) : IRegionalRateRuleRule
{
    public List<string> Check(RegionalRateRule item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional rate rule.</summary>
public sealed record RegionalRateRuleValidator(List<IRegionalRateRuleRule> Rules)
{
    public List<string> Validate(RegionalRateRule item)
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

    public void RequireValid(RegionalRateRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalRateRule> items)
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

/// <summary>Functions of regional rate rule rules.</summary>
public static class RegionalRateRuleRules
{
    public static RegionalRateRuleValidator DefaultRegionalRateRuleValidator()
    {
        return new RegionalRateRuleValidator(
            new List<IRegionalRateRuleRule>
            {
                new RegionalRateRuleCodePrefixRule("OPS"),
                new RegionalRateRuleRequiredCodeRule("regional rate rule needs this field"),
                new RegionalRateRuleLimitRangeRule(64, 86400),
            });
    }
}
