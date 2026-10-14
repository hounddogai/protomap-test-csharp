namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan override calculation.</summary>
public interface ISuccessionPlanOverrideAdjuster
{
    string Describe();
    double Adjust(SuccessionPlanOverride item, double valueValue);
}

public sealed record CapSuccessionPlanOverrideAdjuster(double Cap) : ISuccessionPlanOverrideAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSuccessionPlanOverrideAdjuster(double Step) : ISuccessionPlanOverrideAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SuccessionPlanOverrideRevisionOffsetAdjuster(double Weight) : ISuccessionPlanOverrideAdjuster
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(SuccessionPlanOverride item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record FlaggedSuccessionPlanOverrideAdjuster(Registry Flags, string Flag, double Percent) : ISuccessionPlanOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SuccessionPlanOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSuccessionPlanOverrideAdjuster(double Minimum) : ISuccessionPlanOverrideAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the succession plan override steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanOverrideEngine(List<ISuccessionPlanOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanOverrideEngine DefaultSuccessionPlanOverrideEngine(Registry flags)
    {
        return new SuccessionPlanOverrideEngine(
            new List<ISuccessionPlanOverrideAdjuster>
            {
                new CapSuccessionPlanOverrideAdjuster(34937.0),
                new RoundingSuccessionPlanOverrideAdjuster(0.05),
                new SuccessionPlanOverrideRevisionOffsetAdjuster(0.27),
                new FlaggedSuccessionPlanOverrideAdjuster(flags, "succession-plan-override-rolling", 7.6),
                new MinimumSuccessionPlanOverrideAdjuster(34.2),
            },
            new List<string>(),
            3.9,
            69235.6);
    }

    public double Compute(SuccessionPlanOverride item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
