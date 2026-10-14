namespace Acme.Workforce.Engagement.ActionPlan.Snapshot;

/// <summary>A rule a action plan snapshot must pass. Check returns its problems, or none.</summary>
public interface IActionPlanSnapshotRule
{
    List<string> Check(ActionPlanSnapshot item);
}

public sealed record ActionPlanSnapshotWindowDaysRangeRule(int Min, int Max) : IActionPlanSnapshotRule
{
    public List<string> Check(ActionPlanSnapshot item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanSnapshotLocaleLengthRule(int MaxLength) : IActionPlanSnapshotRule
{
    public List<string> Check(ActionPlanSnapshot item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanSnapshotCodePrefixRule(string Prefix) : IActionPlanSnapshotRule
{
    public List<string> Check(ActionPlanSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanSnapshotTagsLimitRule(int MaxCount, List<string> Banned) : IActionPlanSnapshotRule
{
    public List<string> Check(ActionPlanSnapshot item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record ActionPlanSnapshotLabelLengthRule(int MaxLength) : IActionPlanSnapshotRule
{
    public List<string> Check(ActionPlanSnapshot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan snapshot.</summary>
public sealed record ActionPlanSnapshotValidator(List<IActionPlanSnapshotRule> Rules)
{
    public List<string> Validate(ActionPlanSnapshot item)
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

    public void RequireValid(ActionPlanSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanSnapshot> items)
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

/// <summary>Functions of action plan snapshot rules.</summary>
public static class ActionPlanSnapshotRules
{
    public static ActionPlanSnapshotValidator DefaultActionPlanSnapshotValidator()
    {
        return new ActionPlanSnapshotValidator(
            new List<IActionPlanSnapshotRule>
            {
                new ActionPlanSnapshotWindowDaysRangeRule(16, 80),
                new ActionPlanSnapshotLocaleLengthRule(16),
                new ActionPlanSnapshotCodePrefixRule("X"),
                new ActionPlanSnapshotTagsLimitRule(3600, new List<string> { "manual" }),
                new ActionPlanSnapshotLabelLengthRule(16),
            });
    }
}
