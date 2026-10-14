namespace Acme.Workforce.Talent.Mobility.Manifest;

/// <summary>A rule a scheduled mobility preference must pass. Check returns its problems, or none.</summary>
public interface IScheduledMobilityPreferenceRule
{
    List<string> Check(ScheduledMobilityPreference item);
}

public sealed record ScheduledMobilityPreferenceLabelLengthRule(int MaxLength) : IScheduledMobilityPreferenceRule
{
    public List<string> Check(ScheduledMobilityPreference item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityPreferenceArchivedGuardRule(double Limit) : IScheduledMobilityPreferenceRule
{
    public List<string> Check(ScheduledMobilityPreference item)
    {
        if (item.Archived && item.Hours > Limit)
        {
            return new List<string> { $"hours {item.Hours} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityPreferenceRequiredSalaryBandRule(string Message) : IScheduledMobilityPreferenceRule
{
    public List<string> Check(ScheduledMobilityPreference item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityPreferenceCodeLengthRule(int MaxLength) : IScheduledMobilityPreferenceRule
{
    public List<string> Check(ScheduledMobilityPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled mobility preference.</summary>
public sealed record ScheduledMobilityPreferenceValidator(List<IScheduledMobilityPreferenceRule> Rules)
{
    public List<string> Validate(ScheduledMobilityPreference item)
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

    public void RequireValid(ScheduledMobilityPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledMobilityPreference> items)
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

/// <summary>Functions of scheduled mobility preference rules.</summary>
public static class ScheduledMobilityPreferenceRules
{
    public static ScheduledMobilityPreferenceValidator DefaultScheduledMobilityPreferenceValidator()
    {
        return new ScheduledMobilityPreferenceValidator(
            new List<IScheduledMobilityPreferenceRule>
            {
                new ScheduledMobilityPreferenceLabelLengthRule(32),
                new ScheduledMobilityPreferenceArchivedGuardRule(4.2),
                new ScheduledMobilityPreferenceRequiredSalaryBandRule("scheduled mobility preference needs an entry"),
                new ScheduledMobilityPreferenceCodeLengthRule(255),
            });
    }
}
