namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional exception preference must pass. Check returns its problems, or none.</summary>
public interface IRegionalExceptionPreferenceRule
{
    List<string> Check(RegionalExceptionPreference item);
}

public sealed record RegionalExceptionPreferenceAllowedLabelRule(List<string> Allowed) : IRegionalExceptionPreferenceRule
{
    public List<string> Check(RegionalExceptionPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

public sealed record RegionalExceptionPreferenceRequiredCodeRule(string Message) : IRegionalExceptionPreferenceRule
{
    public List<string> Check(RegionalExceptionPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalExceptionPreferenceCodePrefixRule(string Prefix) : IRegionalExceptionPreferenceRule
{
    public List<string> Check(RegionalExceptionPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalExceptionPreferenceLocaleLengthRule(int MaxLength) : IRegionalExceptionPreferenceRule
{
    public List<string> Check(RegionalExceptionPreference item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalExceptionPreferenceCodeLengthRule(int MaxLength) : IRegionalExceptionPreferenceRule
{
    public List<string> Check(RegionalExceptionPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional exception preference.</summary>
public sealed record RegionalExceptionPreferenceValidator(List<IRegionalExceptionPreferenceRule> Rules)
{
    public List<string> Validate(RegionalExceptionPreference item)
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

    public void RequireValid(RegionalExceptionPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalExceptionPreference> items)
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

/// <summary>Functions of regional exception preference rules.</summary>
public static class RegionalExceptionPreferenceRules
{
    public static RegionalExceptionPreferenceValidator DefaultRegionalExceptionPreferenceValidator()
    {
        return new RegionalExceptionPreferenceValidator(
            new List<IRegionalExceptionPreferenceRule>
            {
                new RegionalExceptionPreferenceAllowedLabelRule(new List<string> { "tiered" }),
                new RegionalExceptionPreferenceRequiredCodeRule("regional exception preference needs this field"),
                new RegionalExceptionPreferenceCodePrefixRule("X"),
                new RegionalExceptionPreferenceLocaleLengthRule(1000),
                new RegionalExceptionPreferenceCodeLengthRule(86400),
            });
    }
}
