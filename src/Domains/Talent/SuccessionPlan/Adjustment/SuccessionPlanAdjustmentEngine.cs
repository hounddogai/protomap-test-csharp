namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan adjustment calculation.</summary>
public interface ISuccessionPlanAdjustmentFactor
{
    string Describe();
    double Adjust(SuccessionPlanAdjustment item, double valueValue);
}

public sealed record CapSuccessionPlanAdjustmentAdjuster(double Cap) : ISuccessionPlanAdjustmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SuccessionPlanAdjustmentRetriesOffsetAdjuster(double Weight) : ISuccessionPlanAdjustmentFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(SuccessionPlanAdjustment item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record PercentSuccessionPlanAdjustmentAdjuster(double Percent) : ISuccessionPlanAdjustmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSuccessionPlanAdjustmentAdjuster(double Minimum) : ISuccessionPlanAdjustmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedSuccessionPlanAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : ISuccessionPlanAdjustmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SuccessionPlanAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleSuccessionPlanAdjustmentAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanAdjustmentFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SuccessionPlanAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the succession plan adjustment steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanAdjustmentEngine(List<ISuccessionPlanAdjustmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanAdjustmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanAdjustmentEngine DefaultSuccessionPlanAdjustmentEngine(Registry flags)
    {
        return new SuccessionPlanAdjustmentEngine(
            new List<ISuccessionPlanAdjustmentFactor>
            {
                new CapSuccessionPlanAdjustmentAdjuster(13990.0),
                new SuccessionPlanAdjustmentRetriesOffsetAdjuster(1.6),
                new PercentSuccessionPlanAdjustmentAdjuster(0.5),
                new MinimumSuccessionPlanAdjustmentAdjuster(9.3),
                new FlaggedSuccessionPlanAdjustmentAdjuster(flags, "succession-plan-adjustment-primary", 4.0),
                new TitleSuccessionPlanAdjustmentAdjuster(new Dictionary<string, double> { ["north"] = 1.01 }),
            },
            new List<string>(),
            0.6,
            39083.5);
    }

    public double Compute(SuccessionPlanAdjustment item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
