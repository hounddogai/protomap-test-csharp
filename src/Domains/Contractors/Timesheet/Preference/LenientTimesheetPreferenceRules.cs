namespace Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>A rule a lenient timesheet preference must pass. Check returns its problems, or none.</summary>
public interface ILenientTimesheetPreferenceRule
{
    List<string> Check(LenientTimesheetPreference item);
}

public sealed record LenientTimesheetPreferenceLimitRangeRule(int Min, int Max) : ILenientTimesheetPreferenceRule
{
    public List<string> Check(LenientTimesheetPreference item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientTimesheetPreferenceRequiredCountryRule(string Message) : ILenientTimesheetPreferenceRule
{
    public List<string> Check(LenientTimesheetPreference item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientTimesheetPreferenceGradeLengthRule(int MaxLength) : ILenientTimesheetPreferenceRule
{
    public List<string> Check(LenientTimesheetPreference item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientTimesheetPreferenceCountryLengthRule(int MaxLength) : ILenientTimesheetPreferenceRule
{
    public List<string> Check(LenientTimesheetPreference item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient timesheet preference.</summary>
public sealed record LenientTimesheetPreferenceValidator(List<ILenientTimesheetPreferenceRule> Rules)
{
    public List<string> Validate(LenientTimesheetPreference item)
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

    public void RequireValid(LenientTimesheetPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientTimesheetPreference> items)
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

/// <summary>Functions of lenient timesheet preference rules.</summary>
public static class LenientTimesheetPreferenceRules
{
    public static LenientTimesheetPreferenceValidator DefaultLenientTimesheetPreferenceValidator()
    {
        return new LenientTimesheetPreferenceValidator(
            new List<ILenientTimesheetPreferenceRule>
            {
                new LenientTimesheetPreferenceLimitRangeRule(16, 120),
                new LenientTimesheetPreferenceRequiredCountryRule("lenient timesheet preference needs this field"),
                new LenientTimesheetPreferenceGradeLengthRule(3600),
                new LenientTimesheetPreferenceCountryLengthRule(1000),
            });
    }
}
