namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the strict succession plan override calculation.</summary>
public interface IStrictSuccessionPlanOverrideAdjuster
{
    string Describe();
    double Adjust(StrictSuccessionPlanOverride item, double valueValue);
}

public sealed record StrictSuccessionPlanOverrideVersionOffsetAdjuster(double Weight) : IStrictSuccessionPlanOverrideAdjuster
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(StrictSuccessionPlanOverride item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record FlaggedStrictSuccessionPlanOverrideAdjuster(Registry Flags, string Flag, double Percent) : IStrictSuccessionPlanOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictSuccessionPlanOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentStrictSuccessionPlanOverrideAdjuster(double Percent) : IStrictSuccessionPlanOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictSuccessionPlanOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryStrictSuccessionPlanOverrideAdjuster(Dictionary<string, double> Factors) : IStrictSuccessionPlanOverrideAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(StrictSuccessionPlanOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the strict succession plan override steps in order and keeps a trace of each.</summary>
public sealed class StrictSuccessionPlanOverrideEngine(List<IStrictSuccessionPlanOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictSuccessionPlanOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictSuccessionPlanOverrideEngine DefaultStrictSuccessionPlanOverrideEngine(Registry flags)
    {
        return new StrictSuccessionPlanOverrideEngine(
            new List<IStrictSuccessionPlanOverrideAdjuster>
            {
                new StrictSuccessionPlanOverrideVersionOffsetAdjuster(0.34),
                new FlaggedStrictSuccessionPlanOverrideAdjuster(flags, "strict-succession-plan-override-legacy", 0.3),
                new PercentStrictSuccessionPlanOverrideAdjuster(5.9),
                new CategoryStrictSuccessionPlanOverrideAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.28, ["retail"] = 0.97 }),
            },
            new List<string>(),
            1.8,
            76511.9);
    }

    public double Price(StrictSuccessionPlanOverride item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StrictSuccessionPlanOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
