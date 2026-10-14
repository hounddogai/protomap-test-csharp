namespace Acme.Workforce.Contractors.Offboarding.Cycle;

/// <summary>A rule a offboarding preference must pass. Check returns its problems, or none.</summary>
public interface IOffboardingPreferenceRule
{
    List<string> Check(OffboardingPreference item);
}

public sealed record OffboardingPreferenceFloorValueRangeRule(double Min, double Max) : IOffboardingPreferenceRule
{
    public List<string> Check(OffboardingPreference item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingPreferenceTeamLengthRule(int MaxLength) : IOffboardingPreferenceRule
{
    public List<string> Check(OffboardingPreference item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingPreferenceRequiredLocationRule(string Message) : IOffboardingPreferenceRule
{
    public List<string> Check(OffboardingPreference item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingPreferenceUpdatedAtRangeRule(int Min, int Max) : IOffboardingPreferenceRule
{
    public List<string> Check(OffboardingPreference item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OffboardingPreferenceRequiredTeamRule(string Message) : IOffboardingPreferenceRule
{
    public List<string> Check(OffboardingPreference item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding preference.</summary>
public sealed record OffboardingPreferenceValidator(List<IOffboardingPreferenceRule> Rules)
{
    public List<string> Validate(OffboardingPreference item)
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

    public void RequireValid(OffboardingPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingPreference> items)
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

/// <summary>Functions of offboarding preference rules.</summary>
public static class OffboardingPreferenceRules
{
    public static OffboardingPreferenceValidator DefaultOffboardingPreferenceValidator()
    {
        return new OffboardingPreferenceValidator(
            new List<IOffboardingPreferenceRule>
            {
                new OffboardingPreferenceFloorValueRangeRule(5.5, 4582.8),
                new OffboardingPreferenceTeamLengthRule(255),
                new OffboardingPreferenceRequiredLocationRule("offboarding preference needs this field"),
                new OffboardingPreferenceUpdatedAtRangeRule(16, 16),
                new OffboardingPreferenceRequiredTeamRule("offboarding preference needs an entry"),
            });
    }
}
