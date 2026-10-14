namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered assignment snapshot must pass. Check returns its problems, or none.</summary>
public interface ITieredAssignmentSnapshotRule
{
    List<string> Check(TieredAssignmentSnapshot item);
}

public sealed record TieredAssignmentSnapshotTimezoneLengthRule(int MaxLength) : ITieredAssignmentSnapshotRule
{
    public List<string> Check(TieredAssignmentSnapshot item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAssignmentSnapshotUpdatedAtRangeRule(int Min, int Max) : ITieredAssignmentSnapshotRule
{
    public List<string> Check(TieredAssignmentSnapshot item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAssignmentSnapshotAllowedCodeRule(List<string> Allowed) : ITieredAssignmentSnapshotRule
{
    public List<string> Check(TieredAssignmentSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record TieredAssignmentSnapshotRetriesRangeRule(int Min, int Max) : ITieredAssignmentSnapshotRule
{
    public List<string> Check(TieredAssignmentSnapshot item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tiered assignment snapshot.</summary>
public sealed record TieredAssignmentSnapshotValidator(List<ITieredAssignmentSnapshotRule> Rules)
{
    public List<string> Validate(TieredAssignmentSnapshot item)
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

    public void RequireValid(TieredAssignmentSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredAssignmentSnapshot> items)
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

/// <summary>Functions of tiered assignment snapshot rules.</summary>
public static class TieredAssignmentSnapshotRules
{
    public static TieredAssignmentSnapshotValidator DefaultTieredAssignmentSnapshotValidator()
    {
        return new TieredAssignmentSnapshotValidator(
            new List<ITieredAssignmentSnapshotRule>
            {
                new TieredAssignmentSnapshotTimezoneLengthRule(16),
                new TieredAssignmentSnapshotUpdatedAtRangeRule(32, 32),
                new TieredAssignmentSnapshotAllowedCodeRule(new List<string> { "seasonal", "weighted" }),
                new TieredAssignmentSnapshotRetriesRangeRule(32, 3600),
            });
    }
}
