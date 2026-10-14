namespace Acme.Workforce.Workplace.Booking.Entry;

/// <summary>A rule a regional booking preference must pass. Check returns its problems, or none.</summary>
public interface IRegionalBookingPreferenceRule
{
    List<string> Check(RegionalBookingPreference item);
}

public sealed record RegionalBookingPreferenceLeaveDaysRangeRule(int Min, int Max) : IRegionalBookingPreferenceRule
{
    public List<string> Check(RegionalBookingPreference item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBookingPreferenceCeilingRangeRule(double Min, double Max) : IRegionalBookingPreferenceRule
{
    public List<string> Check(RegionalBookingPreference item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalBookingPreferenceSkillLengthRule(int MaxLength) : IRegionalBookingPreferenceRule
{
    public List<string> Check(RegionalBookingPreference item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBookingPreferenceSummaryLengthRule(int MaxLength) : IRegionalBookingPreferenceRule
{
    public List<string> Check(RegionalBookingPreference item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBookingPreferenceLabelLengthRule(int MaxLength) : IRegionalBookingPreferenceRule
{
    public List<string> Check(RegionalBookingPreference item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBookingPreferenceRequiredLabelRule(string Message) : IRegionalBookingPreferenceRule
{
    public List<string> Check(RegionalBookingPreference item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional booking preference.</summary>
public sealed record RegionalBookingPreferenceValidator(List<IRegionalBookingPreferenceRule> Rules)
{
    public List<string> Validate(RegionalBookingPreference item)
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

    public void RequireValid(RegionalBookingPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalBookingPreference> items)
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

/// <summary>Functions of regional booking preference rules.</summary>
public static class RegionalBookingPreferenceRules
{
    public static RegionalBookingPreferenceValidator DefaultRegionalBookingPreferenceValidator()
    {
        return new RegionalBookingPreferenceValidator(
            new List<IRegionalBookingPreferenceRule>
            {
                new RegionalBookingPreferenceLeaveDaysRangeRule(32, 80),
                new RegionalBookingPreferenceCeilingRangeRule(4.7, 2801.2),
                new RegionalBookingPreferenceSkillLengthRule(32),
                new RegionalBookingPreferenceSummaryLengthRule(255),
                new RegionalBookingPreferenceLabelLengthRule(120),
                new RegionalBookingPreferenceRequiredLabelRule("regional booking preference needs an entry"),
            });
    }
}
