namespace Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>A rule a town hall preference must pass. Check returns its problems, or none.</summary>
public interface ITownHallPreferenceRule
{
    List<string> Check(TownHallPreference item);
}

public sealed record TownHallPreferenceRateRangeRule(double Min, double Max) : ITownHallPreferenceRule
{
    public List<string> Check(TownHallPreference item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallPreferenceHoursRangeRule(double Min, double Max) : ITownHallPreferenceRule
{
    public List<string> Check(TownHallPreference item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallPreferenceApprovedGuardRule(int Limit) : ITownHallPreferenceRule
{
    public List<string> Check(TownHallPreference item)
    {
        if (item.Approved && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record TownHallPreferenceLabelLengthRule(int MaxLength) : ITownHallPreferenceRule
{
    public List<string> Check(TownHallPreference item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall preference.</summary>
public sealed record TownHallPreferenceValidator(List<ITownHallPreferenceRule> Rules)
{
    public List<string> Validate(TownHallPreference item)
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

    public void RequireValid(TownHallPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallPreference> items)
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

/// <summary>Functions of town hall preference rules.</summary>
public static class TownHallPreferenceRules
{
    public static TownHallPreferenceValidator DefaultTownHallPreferenceValidator()
    {
        return new TownHallPreferenceValidator(
            new List<ITownHallPreferenceRule>
            {
                new TownHallPreferenceRateRangeRule(8.4, 2906.8),
                new TownHallPreferenceHoursRangeRule(5.7, 2839.5),
                new TownHallPreferenceApprovedGuardRule(80),
                new TownHallPreferenceLabelLengthRule(120),
            });
    }
}
