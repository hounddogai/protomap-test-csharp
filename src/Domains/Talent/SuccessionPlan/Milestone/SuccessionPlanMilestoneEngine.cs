namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan milestone calculation.</summary>
public interface ISuccessionPlanMilestoneStep
{
    string Describe();
    double Adjust(SuccessionPlanMilestone item, double valueValue);
}

public sealed record RoundingSuccessionPlanMilestoneAdjuster(double Step) : ISuccessionPlanMilestoneStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SuccessionPlanMilestoneLimitOffsetAdjuster(double Weight) : ISuccessionPlanMilestoneStep
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(SuccessionPlanMilestone item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record FlaggedSuccessionPlanMilestoneAdjuster(Registry Flags, string Flag, double Percent) : ISuccessionPlanMilestoneStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SuccessionPlanMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceSuccessionPlanMilestoneAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanMilestoneStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(SuccessionPlanMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the succession plan milestone steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanMilestoneEngine(List<ISuccessionPlanMilestoneStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanMilestoneStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanMilestoneEngine DefaultSuccessionPlanMilestoneEngine(Registry flags)
    {
        return new SuccessionPlanMilestoneEngine(
            new List<ISuccessionPlanMilestoneStep>
            {
                new RoundingSuccessionPlanMilestoneAdjuster(1.0),
                new SuccessionPlanMilestoneLimitOffsetAdjuster(1.74),
                new FlaggedSuccessionPlanMilestoneAdjuster(flags, "succession-plan-milestone-lenient", -0.9),
                new SourceSuccessionPlanMilestoneAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.04, ["north"] = 1.09 }),
            },
            new List<string>(),
            1.3,
            51636.3);
    }

    public double Compute(SuccessionPlanMilestone item)
    {
        var current = item.WeightedLoad();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
