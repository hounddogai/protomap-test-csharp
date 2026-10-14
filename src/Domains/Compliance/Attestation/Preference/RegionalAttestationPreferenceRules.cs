namespace Acme.Workforce.Compliance.Attestation.Preference;

/// <summary>A rule a regional attestation preference must pass. Check returns its problems, or none.</summary>
public interface IRegionalAttestationPreferenceRule
{
    List<string> Check(RegionalAttestationPreference item);
}

public sealed record RegionalAttestationPreferenceTitleLengthRule(int MaxLength) : IRegionalAttestationPreferenceRule
{
    public List<string> Check(RegionalAttestationPreference item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAttestationPreferenceRequiredTimezoneRule(string Message) : IRegionalAttestationPreferenceRule
{
    public List<string> Check(RegionalAttestationPreference item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalAttestationPreferenceExpiresAtRangeRule(int Min, int Max) : IRegionalAttestationPreferenceRule
{
    public List<string> Check(RegionalAttestationPreference item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalAttestationPreferenceRequiredTitleRule(string Message) : IRegionalAttestationPreferenceRule
{
    public List<string> Check(RegionalAttestationPreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalAttestationPreferenceBudgetRangeRule(double Min, double Max) : IRegionalAttestationPreferenceRule
{
    public List<string> Check(RegionalAttestationPreference item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAttestationPreferenceRequiredCodeRule(string Message) : IRegionalAttestationPreferenceRule
{
    public List<string> Check(RegionalAttestationPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional attestation preference.</summary>
public sealed record RegionalAttestationPreferenceValidator(List<IRegionalAttestationPreferenceRule> Rules)
{
    public List<string> Validate(RegionalAttestationPreference item)
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

    public void RequireValid(RegionalAttestationPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalAttestationPreference> items)
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

/// <summary>Functions of regional attestation preference rules.</summary>
public static class RegionalAttestationPreferenceRules
{
    public static RegionalAttestationPreferenceValidator DefaultRegionalAttestationPreferenceValidator()
    {
        return new RegionalAttestationPreferenceValidator(
            new List<IRegionalAttestationPreferenceRule>
            {
                new RegionalAttestationPreferenceTitleLengthRule(80),
                new RegionalAttestationPreferenceRequiredTimezoneRule(
                    "regional attestation preference needs an entry"),
                new RegionalAttestationPreferenceExpiresAtRangeRule(120, 255),
                new RegionalAttestationPreferenceRequiredTitleRule(
                    "regional attestation preference needs this field"),
                new RegionalAttestationPreferenceBudgetRangeRule(3.3, 2969.7),
                new RegionalAttestationPreferenceRequiredCodeRule("regional attestation preference needs an entry"),
            });
    }
}
