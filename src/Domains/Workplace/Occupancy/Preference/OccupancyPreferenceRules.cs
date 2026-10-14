namespace Acme.Workforce.Workplace.Occupancy.Preference;

/// <summary>A rule a occupancy preference must pass. Check returns its problems, or none.</summary>
public interface IOccupancyPreferenceRule
{
    List<string> Check(OccupancyPreference item);
}

public sealed record OccupancyPreferenceRequiredCodeRule(string Message) : IOccupancyPreferenceRule
{
    public List<string> Check(OccupancyPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyPreferencePinnedGuardRule(double Limit) : IOccupancyPreferenceRule
{
    public List<string> Check(OccupancyPreference item)
    {
        if (item.Pinned && item.Discount > Limit)
        {
            return new List<string> { $"discount {item.Discount} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyPreferenceCodeLengthRule(int MaxLength) : IOccupancyPreferenceRule
{
    public List<string> Check(OccupancyPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyPreferenceHoursRangeRule(double Min, double Max) : IOccupancyPreferenceRule
{
    public List<string> Check(OccupancyPreference item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy preference.</summary>
public sealed record OccupancyPreferenceValidator(List<IOccupancyPreferenceRule> Rules)
{
    public List<string> Validate(OccupancyPreference item)
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

    public void RequireValid(OccupancyPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyPreference> items)
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

/// <summary>Functions of occupancy preference rules.</summary>
public static class OccupancyPreferenceRules
{
    public static OccupancyPreferenceValidator DefaultOccupancyPreferenceValidator()
    {
        return new OccupancyPreferenceValidator(
            new List<IOccupancyPreferenceRule>
            {
                new OccupancyPreferenceRequiredCodeRule("occupancy preference needs a value"),
                new OccupancyPreferencePinnedGuardRule(3.6),
                new OccupancyPreferenceCodeLengthRule(80),
                new OccupancyPreferenceHoursRangeRule(7.9, 3266.1),
            });
    }
}
