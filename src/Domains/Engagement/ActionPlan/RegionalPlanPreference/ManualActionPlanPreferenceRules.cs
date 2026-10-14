namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual action plan preference must pass. Check returns its problems, or none.</summary>
public interface IManualActionPlanPreferenceRule
{
    List<string> Check(ManualActionPlanPreference item);
}

public sealed record ManualActionPlanPreferenceBatchSizeRangeRule(int Min, int Max) : IManualActionPlanPreferenceRule
{
    public List<string> Check(ManualActionPlanPreference item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualActionPlanPreferenceRequiredChecksumRule(string Message) : IManualActionPlanPreferenceRule
{
    public List<string> Check(ManualActionPlanPreference item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualActionPlanPreferenceRatioRangeRule(double Min, double Max) : IManualActionPlanPreferenceRule
{
    public List<string> Check(ManualActionPlanPreference item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualActionPlanPreferenceChecksumLengthRule(int MaxLength) : IManualActionPlanPreferenceRule
{
    public List<string> Check(ManualActionPlanPreference item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualActionPlanPreferenceAllowedChecksumRule(List<string> Allowed) : IManualActionPlanPreferenceRule
{
    public List<string> Check(ManualActionPlanPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record ManualActionPlanPreferenceNameLengthRule(int MaxLength) : IManualActionPlanPreferenceRule
{
    public List<string> Check(ManualActionPlanPreference item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual action plan preference.</summary>
public sealed record ManualActionPlanPreferenceValidator(List<IManualActionPlanPreferenceRule> Rules)
{
    public List<string> Validate(ManualActionPlanPreference item)
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

    public void RequireValid(ManualActionPlanPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualActionPlanPreference> items)
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

/// <summary>Functions of manual action plan preference rules.</summary>
public static class ManualActionPlanPreferenceRules
{
    public static ManualActionPlanPreferenceValidator DefaultManualActionPlanPreferenceValidator()
    {
        return new ManualActionPlanPreferenceValidator(
            new List<IManualActionPlanPreferenceRule>
            {
                new ManualActionPlanPreferenceBatchSizeRangeRule(1000, 3600),
                new ManualActionPlanPreferenceRequiredChecksumRule("manual action plan preference needs an entry"),
                new ManualActionPlanPreferenceRatioRangeRule(4.7, 1268.9),
                new ManualActionPlanPreferenceChecksumLengthRule(16),
                new ManualActionPlanPreferenceAllowedChecksumRule(new List<string> { "scheduled" }),
                new ManualActionPlanPreferenceNameLengthRule(32),
            });
    }
}
