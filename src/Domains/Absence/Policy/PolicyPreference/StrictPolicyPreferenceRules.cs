namespace Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>A rule a strict policy preference must pass. Check returns its problems, or none.</summary>
public interface IStrictPolicyPreferenceRule
{
    List<string> Check(StrictPolicyPreference item);
}

public sealed record StrictPolicyPreferenceCodePrefixRule(string Prefix) : IStrictPolicyPreferenceRule
{
    public List<string> Check(StrictPolicyPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyPreferenceChannelLengthRule(int MaxLength) : IStrictPolicyPreferenceRule
{
    public List<string> Check(StrictPolicyPreference item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyPreferenceRequiredChannelRule(string Message) : IStrictPolicyPreferenceRule
{
    public List<string> Check(StrictPolicyPreference item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyPreferenceRequiredSourceRule(string Message) : IStrictPolicyPreferenceRule
{
    public List<string> Check(StrictPolicyPreference item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyPreferenceCodeLengthRule(int MaxLength) : IStrictPolicyPreferenceRule
{
    public List<string> Check(StrictPolicyPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyPreferenceTitleLengthRule(int MaxLength) : IStrictPolicyPreferenceRule
{
    public List<string> Check(StrictPolicyPreference item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict policy preference.</summary>
public sealed record StrictPolicyPreferenceValidator(List<IStrictPolicyPreferenceRule> Rules)
{
    public List<string> Validate(StrictPolicyPreference item)
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

    public void RequireValid(StrictPolicyPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPolicyPreference> items)
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

/// <summary>Functions of strict policy preference rules.</summary>
public static class StrictPolicyPreferenceRules
{
    public static StrictPolicyPreferenceValidator DefaultStrictPolicyPreferenceValidator()
    {
        return new StrictPolicyPreferenceValidator(
            new List<IStrictPolicyPreferenceRule>
            {
                new StrictPolicyPreferenceCodePrefixRule("OPS"),
                new StrictPolicyPreferenceChannelLengthRule(80),
                new StrictPolicyPreferenceRequiredChannelRule("strict policy preference needs a value"),
                new StrictPolicyPreferenceRequiredSourceRule("strict policy preference needs a value"),
                new StrictPolicyPreferenceCodeLengthRule(32),
                new StrictPolicyPreferenceTitleLengthRule(120),
            });
    }
}
