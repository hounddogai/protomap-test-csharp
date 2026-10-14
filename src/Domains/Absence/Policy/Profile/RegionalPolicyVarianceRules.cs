namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional policy variance must pass. Check returns its problems, or none.</summary>
public interface IRegionalPolicyVarianceRule
{
    List<string> Check(RegionalPolicyVariance item);
}

public sealed record RegionalPolicyVarianceCodePrefixRule(string Prefix) : IRegionalPolicyVarianceRule
{
    public List<string> Check(RegionalPolicyVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPolicyVarianceVisibleGuardRule(double Limit) : IRegionalPolicyVarianceRule
{
    public List<string> Check(RegionalPolicyVariance item)
    {
        if (item.Visible && item.Hours > Limit)
        {
            return new List<string> { $"hours {item.Hours} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPolicyVarianceEndsAtRangeRule(int Min, int Max) : IRegionalPolicyVarianceRule
{
    public List<string> Check(RegionalPolicyVariance item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPolicyVariancePriorityRangeRule(int Min, int Max) : IRegionalPolicyVarianceRule
{
    public List<string> Check(RegionalPolicyVariance item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPolicyVarianceAllowedNameRule(List<string> Allowed) : IRegionalPolicyVarianceRule
{
    public List<string> Check(RegionalPolicyVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional policy variance.</summary>
public sealed record RegionalPolicyVarianceValidator(List<IRegionalPolicyVarianceRule> Rules)
{
    public List<string> Validate(RegionalPolicyVariance item)
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

    public void RequireValid(RegionalPolicyVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalPolicyVariance> items)
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

/// <summary>Functions of regional policy variance rules.</summary>
public static class RegionalPolicyVarianceRules
{
    public static RegionalPolicyVarianceValidator DefaultRegionalPolicyVarianceValidator()
    {
        return new RegionalPolicyVarianceValidator(
            new List<IRegionalPolicyVarianceRule>
            {
                new RegionalPolicyVarianceCodePrefixRule("X"),
                new RegionalPolicyVarianceVisibleGuardRule(5.7),
                new RegionalPolicyVarianceEndsAtRangeRule(3600, 86400),
                new RegionalPolicyVariancePriorityRangeRule(32, 120),
                new RegionalPolicyVarianceAllowedNameRule(new List<string> { "primary", "regional", "scheduled" }),
            });
    }
}
