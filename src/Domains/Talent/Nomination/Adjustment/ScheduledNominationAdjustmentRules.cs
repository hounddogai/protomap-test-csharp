namespace Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>A rule a scheduled nomination adjustment must pass. Check returns its problems, or none.</summary>
public interface IScheduledNominationAdjustmentRule
{
    List<string> Check(ScheduledNominationAdjustment item);
}

public sealed record ScheduledNominationAdjustmentExternalRefLengthRule(int MaxLength) : IScheduledNominationAdjustmentRule
{
    public List<string> Check(ScheduledNominationAdjustment item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledNominationAdjustmentFloorValueRangeRule(double Min, double Max) : IScheduledNominationAdjustmentRule
{
    public List<string> Check(ScheduledNominationAdjustment item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledNominationAdjustmentRequiredLabelRule(string Message) : IScheduledNominationAdjustmentRule
{
    public List<string> Check(ScheduledNominationAdjustment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledNominationAdjustmentTagsLimitRule(int MaxCount, List<string> Banned) : IScheduledNominationAdjustmentRule
{
    public List<string> Check(ScheduledNominationAdjustment item)
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

/// <summary>Runs every rule of a scheduled nomination adjustment.</summary>
public sealed record ScheduledNominationAdjustmentValidator(List<IScheduledNominationAdjustmentRule> Rules)
{
    public List<string> Validate(ScheduledNominationAdjustment item)
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

    public void RequireValid(ScheduledNominationAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledNominationAdjustment> items)
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

/// <summary>Functions of scheduled nomination adjustment rules.</summary>
public static class ScheduledNominationAdjustmentRules
{
    public static ScheduledNominationAdjustmentValidator DefaultScheduledNominationAdjustmentValidator()
    {
        return new ScheduledNominationAdjustmentValidator(
            new List<IScheduledNominationAdjustmentRule>
            {
                new ScheduledNominationAdjustmentExternalRefLengthRule(64),
                new ScheduledNominationAdjustmentFloorValueRangeRule(1.7, 1819.0),
                new ScheduledNominationAdjustmentRequiredLabelRule(
                    "scheduled nomination adjustment needs this field"),
                new ScheduledNominationAdjustmentTagsLimitRule(32, new List<string> { "capped", "fallback" }),
            });
    }
}
