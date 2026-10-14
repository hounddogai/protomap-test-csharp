namespace Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>A rule a rolling merit cycle segment must pass. Check returns its problems, or none.</summary>
public interface IRollingMeritCycleSegmentRule
{
    List<string> Check(RollingMeritCycleSegment item);
}

public sealed record RollingMeritCycleSegmentMaximumRangeRule(double Min, double Max) : IRollingMeritCycleSegmentRule
{
    public List<string> Check(RollingMeritCycleSegment item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMeritCycleSegmentLabelLengthRule(int MaxLength) : IRollingMeritCycleSegmentRule
{
    public List<string> Check(RollingMeritCycleSegment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMeritCycleSegmentTagsLimitRule(int MaxCount, List<string> Banned) : IRollingMeritCycleSegmentRule
{
    public List<string> Check(RollingMeritCycleSegment item)
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

/// <summary>Runs every rule of a rolling merit cycle segment.</summary>
public sealed record RollingMeritCycleSegmentValidator(List<IRollingMeritCycleSegmentRule> Rules)
{
    public List<string> Validate(RollingMeritCycleSegment item)
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

    public void RequireValid(RollingMeritCycleSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingMeritCycleSegment> items)
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

/// <summary>Functions of rolling merit cycle segment rules.</summary>
public static class RollingMeritCycleSegmentRules
{
    public static RollingMeritCycleSegmentValidator DefaultRollingMeritCycleSegmentValidator()
    {
        return new RollingMeritCycleSegmentValidator(
            new List<IRollingMeritCycleSegmentRule>
            {
                new RollingMeritCycleSegmentMaximumRangeRule(5.3, 1107.6),
                new RollingMeritCycleSegmentLabelLengthRule(3600),
                new RollingMeritCycleSegmentTagsLimitRule(80, new List<string> { "lenient", "batched", "weighted" }),
            });
    }
}
