namespace Acme.Workforce.Engagement.ActionPlan.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default action plan rollup calculation.</summary>
public interface IDefaultActionPlanRollupStep
{
    string Describe();
    double Adjust(DefaultActionPlanRollup item, double valueValue);
}

public sealed record TeamDefaultActionPlanRollupAdjuster(Dictionary<string, double> Factors) : IDefaultActionPlanRollupStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(DefaultActionPlanRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDefaultActionPlanRollupAdjuster(double Step) : IDefaultActionPlanRollupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultActionPlanRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultActionPlanRollupAdjuster(double Percent) : IDefaultActionPlanRollupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultActionPlanRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DefaultActionPlanRollupExpiresAtOffsetAdjuster(double Weight) : IDefaultActionPlanRollupStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(DefaultActionPlanRollup item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record CapDefaultActionPlanRollupAdjuster(double Cap) : IDefaultActionPlanRollupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultActionPlanRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDefaultActionPlanRollupAdjuster(Registry Flags, string Flag, double Percent) : IDefaultActionPlanRollupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultActionPlanRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default action plan rollup steps in order and keeps a trace of each.</summary>
public sealed class DefaultActionPlanRollupEngine(List<IDefaultActionPlanRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultActionPlanRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultActionPlanRollupEngine DefaultDefaultActionPlanRollupEngine(Registry flags)
    {
        return new DefaultActionPlanRollupEngine(
            new List<IDefaultActionPlanRollupStep>
            {
                new TeamDefaultActionPlanRollupAdjuster(new Dictionary<string, double> { ["east"] = 0.77 }),
                new RoundingDefaultActionPlanRollupAdjuster(0.5),
                new PercentDefaultActionPlanRollupAdjuster(1.8),
                new DefaultActionPlanRollupExpiresAtOffsetAdjuster(0.61),
                new CapDefaultActionPlanRollupAdjuster(41376.0),
                new FlaggedDefaultActionPlanRollupAdjuster(flags, "default-action-plan-rollup-weighted", 9.4),
            },
            new List<string>(),
            4.3,
            91986.6);
    }

    public double Compute(DefaultActionPlanRollup item)
    {
        var current = item.ProjectedTotal();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<DefaultActionPlanRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
