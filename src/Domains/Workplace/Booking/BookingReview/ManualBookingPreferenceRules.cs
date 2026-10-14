namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual booking preference must pass. Check returns its problems, or none.</summary>
public interface IManualBookingPreferenceRule
{
    List<string> Check(ManualBookingPreference item);
}

public sealed record ManualBookingPreferenceLabelLengthRule(int MaxLength) : IManualBookingPreferenceRule
{
    public List<string> Check(ManualBookingPreference item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBookingPreferenceAllowedEmployeeIdRule(List<string> Allowed) : IManualBookingPreferenceRule
{
    public List<string> Check(ManualBookingPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record ManualBookingPreferenceRequiredCodeRule(string Message) : IManualBookingPreferenceRule
{
    public List<string> Check(ManualBookingPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualBookingPreferenceCodeLengthRule(int MaxLength) : IManualBookingPreferenceRule
{
    public List<string> Check(ManualBookingPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBookingPreferenceRequiredLabelRule(string Message) : IManualBookingPreferenceRule
{
    public List<string> Check(ManualBookingPreference item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual booking preference.</summary>
public sealed record ManualBookingPreferenceValidator(List<IManualBookingPreferenceRule> Rules)
{
    public List<string> Validate(ManualBookingPreference item)
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

    public void RequireValid(ManualBookingPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualBookingPreference> items)
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

/// <summary>Functions of manual booking preference rules.</summary>
public static class ManualBookingPreferenceRules
{
    public static ManualBookingPreferenceValidator DefaultManualBookingPreferenceValidator()
    {
        return new ManualBookingPreferenceValidator(
            new List<IManualBookingPreferenceRule>
            {
                new ManualBookingPreferenceLabelLengthRule(120),
                new ManualBookingPreferenceAllowedEmployeeIdRule(new List<string> { "adaptive" }),
                new ManualBookingPreferenceRequiredCodeRule("manual booking preference needs an entry"),
                new ManualBookingPreferenceCodeLengthRule(120),
                new ManualBookingPreferenceRequiredLabelRule("manual booking preference needs this field"),
            });
    }
}
