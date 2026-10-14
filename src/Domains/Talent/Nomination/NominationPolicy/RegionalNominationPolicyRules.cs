namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional nomination policy must pass. Check returns its problems, or none.</summary>
public interface IRegionalNominationPolicyRule
{
    List<string> Check(RegionalNominationPolicy item);
}

public sealed record RegionalNominationPolicyRequiredChecksumRule(string Message) : IRegionalNominationPolicyRule
{
    public List<string> Check(RegionalNominationPolicy item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalNominationPolicyFteRangeRule(double Min, double Max) : IRegionalNominationPolicyRule
{
    public List<string> Check(RegionalNominationPolicy item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalNominationPolicyAllowedCategoryRule(List<string> Allowed) : IRegionalNominationPolicyRule
{
    public List<string> Check(RegionalNominationPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is too long" };
        }
        return new List<string>();
    }
}

public sealed record RegionalNominationPolicyNameLengthRule(int MaxLength) : IRegionalNominationPolicyRule
{
    public List<string> Check(RegionalNominationPolicy item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalNominationPolicyCodePrefixRule(string Prefix) : IRegionalNominationPolicyRule
{
    public List<string> Check(RegionalNominationPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalNominationPolicyRequiredNameRule(string Message) : IRegionalNominationPolicyRule
{
    public List<string> Check(RegionalNominationPolicy item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional nomination policy.</summary>
public sealed record RegionalNominationPolicyValidator(List<IRegionalNominationPolicyRule> Rules)
{
    public List<string> Validate(RegionalNominationPolicy item)
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

    public void RequireValid(RegionalNominationPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalNominationPolicy> items)
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

/// <summary>Functions of regional nomination policy rules.</summary>
public static class RegionalNominationPolicyRules
{
    public static RegionalNominationPolicyValidator DefaultRegionalNominationPolicyValidator()
    {
        return new RegionalNominationPolicyValidator(
            new List<IRegionalNominationPolicyRule>
            {
                new RegionalNominationPolicyRequiredChecksumRule("regional nomination policy needs a value"),
                new RegionalNominationPolicyFteRangeRule(3.2, 731.2),
                new RegionalNominationPolicyAllowedCategoryRule(new List<string> { "manual", "regional", "default" }),
                new RegionalNominationPolicyNameLengthRule(64),
                new RegionalNominationPolicyCodePrefixRule("Q"),
                new RegionalNominationPolicyRequiredNameRule("regional nomination policy needs an entry"),
            });
    }
}
