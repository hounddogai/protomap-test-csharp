namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a merit cycle milestone must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleMilestoneRule
{
    List<string> Check(MeritCycleMilestone item);
}

public sealed record MeritCycleMilestoneLabelLengthRule(int MaxLength) : IMeritCycleMilestoneRule
{
    public List<string> Check(MeritCycleMilestone item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleMilestoneVisibleGuardRule(double Limit) : IMeritCycleMilestoneRule
{
    public List<string> Check(MeritCycleMilestone item)
    {
        if (item.Visible && item.Threshold > Limit)
        {
            return new List<string> { $"threshold {item.Threshold} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleMilestoneRevisionRangeRule(int Min, int Max) : IMeritCycleMilestoneRule
{
    public List<string> Check(MeritCycleMilestone item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MeritCycleMilestoneSummaryLengthRule(int MaxLength) : IMeritCycleMilestoneRule
{
    public List<string> Check(MeritCycleMilestone item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleMilestoneAllowedLabelRule(List<string> Allowed) : IMeritCycleMilestoneRule
{
    public List<string> Check(MeritCycleMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is too long" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleMilestoneBatchSizeRangeRule(int Min, int Max) : IMeritCycleMilestoneRule
{
    public List<string> Check(MeritCycleMilestone item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a merit cycle milestone.</summary>
public sealed record MeritCycleMilestoneValidator(List<IMeritCycleMilestoneRule> Rules)
{
    public List<string> Validate(MeritCycleMilestone item)
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

    public void RequireValid(MeritCycleMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleMilestone> items)
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

/// <summary>Functions of merit cycle milestone rules.</summary>
public static class MeritCycleMilestoneRules
{
    public static MeritCycleMilestoneValidator DefaultMeritCycleMilestoneValidator()
    {
        return new MeritCycleMilestoneValidator(
            new List<IMeritCycleMilestoneRule>
            {
                new MeritCycleMilestoneLabelLengthRule(1000),
                new MeritCycleMilestoneVisibleGuardRule(1.8),
                new MeritCycleMilestoneRevisionRangeRule(32, 86400),
                new MeritCycleMilestoneSummaryLengthRule(32),
                new MeritCycleMilestoneAllowedLabelRule(new List<string> { "batched", "pending" }),
                new MeritCycleMilestoneBatchSizeRangeRule(80, 80),
            });
    }
}
