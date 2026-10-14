namespace Acme.Workforce.Absence.Holiday.HolidayPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional holiday policy must pass. Check returns its problems, or none.</summary>
public interface IRegionalHolidayPolicyRule
{
    List<string> Check(RegionalHolidayPolicy item);
}

public sealed record RegionalHolidayPolicyReviewerLengthRule(int MaxLength) : IRegionalHolidayPolicyRule
{
    public List<string> Check(RegionalHolidayPolicy item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalHolidayPolicyChannelLengthRule(int MaxLength) : IRegionalHolidayPolicyRule
{
    public List<string> Check(RegionalHolidayPolicy item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalHolidayPolicyAllowedLocaleRule(List<string> Allowed) : IRegionalHolidayPolicyRule
{
    public List<string> Check(RegionalHolidayPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Locale)))
        {
            return new List<string> { $"locale {item.Locale} is too long" };
        }
        return new List<string>();
    }
}

public sealed record RegionalHolidayPolicyCodePrefixRule(string Prefix) : IRegionalHolidayPolicyRule
{
    public List<string> Check(RegionalHolidayPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional holiday policy.</summary>
public sealed record RegionalHolidayPolicyValidator(List<IRegionalHolidayPolicyRule> Rules)
{
    public List<string> Validate(RegionalHolidayPolicy item)
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

    public void RequireValid(RegionalHolidayPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalHolidayPolicy> items)
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

/// <summary>Functions of regional holiday policy rules.</summary>
public static class RegionalHolidayPolicyRules
{
    public static RegionalHolidayPolicyValidator DefaultRegionalHolidayPolicyValidator()
    {
        return new RegionalHolidayPolicyValidator(
            new List<IRegionalHolidayPolicyRule>
            {
                new RegionalHolidayPolicyReviewerLengthRule(32),
                new RegionalHolidayPolicyChannelLengthRule(86400),
                new RegionalHolidayPolicyAllowedLocaleRule(new List<string>()),
                new RegionalHolidayPolicyCodePrefixRule("ACM"),
            });
    }
}
