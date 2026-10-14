namespace Acme.Workforce.Contractors.Rate.Snapshot;

/// <summary>A rule a seasonal rate snapshot must pass. Check returns its problems, or none.</summary>
public interface ISeasonalRateSnapshotRule
{
    List<string> Check(SeasonalRateSnapshot item);
}

public sealed record SeasonalRateSnapshotChannelLengthRule(int MaxLength) : ISeasonalRateSnapshotRule
{
    public List<string> Check(SeasonalRateSnapshot item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalRateSnapshotUpdatedAtRangeRule(int Min, int Max) : ISeasonalRateSnapshotRule
{
    public List<string> Check(SeasonalRateSnapshot item)
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

public sealed record SeasonalRateSnapshotRequiredCodeRule(string Message) : ISeasonalRateSnapshotRule
{
    public List<string> Check(SeasonalRateSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal rate snapshot.</summary>
public sealed record SeasonalRateSnapshotValidator(List<ISeasonalRateSnapshotRule> Rules)
{
    public List<string> Validate(SeasonalRateSnapshot item)
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

    public void RequireValid(SeasonalRateSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalRateSnapshot> items)
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

/// <summary>Functions of seasonal rate snapshot rules.</summary>
public static class SeasonalRateSnapshotRules
{
    public static SeasonalRateSnapshotValidator DefaultSeasonalRateSnapshotValidator()
    {
        return new SeasonalRateSnapshotValidator(
            new List<ISeasonalRateSnapshotRule>
            {
                new SeasonalRateSnapshotChannelLengthRule(120),
                new SeasonalRateSnapshotUpdatedAtRangeRule(32, 3600),
                new SeasonalRateSnapshotRequiredCodeRule("seasonal rate snapshot needs this field"),
            });
    }
}
