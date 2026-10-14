namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>A rule a manual policy preference must pass. Check returns its problems, or none.</summary>
public interface IManualPolicyPreferenceRule
{
    List<string> Check(ManualPolicyPreference item);
}

public sealed record ManualPolicyPreferenceRequiredCodeRule(string Message) : IManualPolicyPreferenceRule
{
    public List<string> Check(ManualPolicyPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualPolicyPreferenceRequiredLabelRule(string Message) : IManualPolicyPreferenceRule
{
    public List<string> Check(ManualPolicyPreference item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualPolicyPreferenceLabelLengthRule(int MaxLength) : IManualPolicyPreferenceRule
{
    public List<string> Check(ManualPolicyPreference item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPolicyPreferenceCodeLengthRule(int MaxLength) : IManualPolicyPreferenceRule
{
    public List<string> Check(ManualPolicyPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPolicyPreferenceRevisionRangeRule(int Min, int Max) : IManualPolicyPreferenceRule
{
    public List<string> Check(ManualPolicyPreference item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual policy preference.</summary>
public sealed record ManualPolicyPreferenceValidator(List<IManualPolicyPreferenceRule> Rules)
{
    public List<string> Validate(ManualPolicyPreference item)
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

    public void RequireValid(ManualPolicyPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualPolicyPreference> items)
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

/// <summary>Functions of manual policy preference rules.</summary>
public static class ManualPolicyPreferenceRules
{
    public static ManualPolicyPreferenceValidator DefaultManualPolicyPreferenceValidator()
    {
        return new ManualPolicyPreferenceValidator(
            new List<IManualPolicyPreferenceRule>
            {
                new ManualPolicyPreferenceRequiredCodeRule("manual policy preference needs this field"),
                new ManualPolicyPreferenceRequiredLabelRule("manual policy preference needs this field"),
                new ManualPolicyPreferenceLabelLengthRule(64),
                new ManualPolicyPreferenceCodeLengthRule(255),
                new ManualPolicyPreferenceRevisionRangeRule(80, 86400),
            });
    }
}
