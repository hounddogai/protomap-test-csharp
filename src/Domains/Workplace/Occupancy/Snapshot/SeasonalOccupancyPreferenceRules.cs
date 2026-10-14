namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>A rule a seasonal occupancy preference must pass. Check returns its problems, or none.</summary>
public interface ISeasonalOccupancyPreferenceRule
{
    List<string> Check(SeasonalOccupancyPreference item);
}

public sealed record SeasonalOccupancyPreferenceRequiredLabelRule(string Message) : ISeasonalOccupancyPreferenceRule
{
    public List<string> Check(SeasonalOccupancyPreference item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalOccupancyPreferenceCodeLengthRule(int MaxLength) : ISeasonalOccupancyPreferenceRule
{
    public List<string> Check(SeasonalOccupancyPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalOccupancyPreferenceCodePrefixRule(string Prefix) : ISeasonalOccupancyPreferenceRule
{
    public List<string> Check(SeasonalOccupancyPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal occupancy preference.</summary>
public sealed record SeasonalOccupancyPreferenceValidator(List<ISeasonalOccupancyPreferenceRule> Rules)
{
    public List<string> Validate(SeasonalOccupancyPreference item)
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

    public void RequireValid(SeasonalOccupancyPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalOccupancyPreference> items)
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

/// <summary>Functions of seasonal occupancy preference rules.</summary>
public static class SeasonalOccupancyPreferenceRules
{
    public static SeasonalOccupancyPreferenceValidator DefaultSeasonalOccupancyPreferenceValidator()
    {
        return new SeasonalOccupancyPreferenceValidator(
            new List<ISeasonalOccupancyPreferenceRule>
            {
                new SeasonalOccupancyPreferenceRequiredLabelRule("seasonal occupancy preference needs a value"),
                new SeasonalOccupancyPreferenceCodeLengthRule(3600),
                new SeasonalOccupancyPreferenceCodePrefixRule("Q"),
            });
    }
}
