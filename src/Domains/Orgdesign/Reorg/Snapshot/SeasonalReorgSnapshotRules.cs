namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>A rule a seasonal reorg snapshot must pass. Check returns its problems, or none.</summary>
public interface ISeasonalReorgSnapshotRule
{
    List<string> Check(SeasonalReorgSnapshot item);
}

public sealed record SeasonalReorgSnapshotAmountRangeRule(double Min, double Max) : ISeasonalReorgSnapshotRule
{
    public List<string> Check(SeasonalReorgSnapshot item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalReorgSnapshotRequiredLabelRule(string Message) : ISeasonalReorgSnapshotRule
{
    public List<string> Check(SeasonalReorgSnapshot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalReorgSnapshotEmployeeIdLengthRule(int MaxLength) : ISeasonalReorgSnapshotRule
{
    public List<string> Check(SeasonalReorgSnapshot item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalReorgSnapshotRequiredLocationRule(string Message) : ISeasonalReorgSnapshotRule
{
    public List<string> Check(SeasonalReorgSnapshot item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal reorg snapshot.</summary>
public sealed record SeasonalReorgSnapshotValidator(List<ISeasonalReorgSnapshotRule> Rules)
{
    public List<string> Validate(SeasonalReorgSnapshot item)
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

    public void RequireValid(SeasonalReorgSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalReorgSnapshot> items)
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

/// <summary>Functions of seasonal reorg snapshot rules.</summary>
public static class SeasonalReorgSnapshotRules
{
    public static SeasonalReorgSnapshotValidator DefaultSeasonalReorgSnapshotValidator()
    {
        return new SeasonalReorgSnapshotValidator(
            new List<ISeasonalReorgSnapshotRule>
            {
                new SeasonalReorgSnapshotAmountRangeRule(2.5, 1532.9),
                new SeasonalReorgSnapshotRequiredLabelRule("seasonal reorg snapshot needs a value"),
                new SeasonalReorgSnapshotEmployeeIdLengthRule(255),
                new SeasonalReorgSnapshotRequiredLocationRule("seasonal reorg snapshot needs a value"),
            });
    }
}
