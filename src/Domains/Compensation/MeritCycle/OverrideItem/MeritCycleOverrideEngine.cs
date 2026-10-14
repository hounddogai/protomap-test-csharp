namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle override calculation.</summary>
public interface IMeritCycleOverrideModifier
{
    string Describe();
    double Adjust(MeritCycleOverride item, double valueValue);
}

public sealed record PercentMeritCycleOverrideAdjuster(double Percent) : IMeritCycleOverrideModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedMeritCycleOverrideAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleOverrideModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapMeritCycleOverrideAdjuster(double Cap) : IMeritCycleOverrideModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCycleOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumMeritCycleOverrideAdjuster(double Minimum) : IMeritCycleOverrideModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCycleOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the merit cycle override steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleOverrideEngine(List<IMeritCycleOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleOverrideEngine DefaultMeritCycleOverrideEngine(Registry flags)
    {
        return new MeritCycleOverrideEngine(
            new List<IMeritCycleOverrideModifier>
            {
                new PercentMeritCycleOverrideAdjuster(3.7),
                new FlaggedMeritCycleOverrideAdjuster(flags, "merit-cycle-override-batched", -6.1),
                new CapMeritCycleOverrideAdjuster(14242.0),
                new MinimumMeritCycleOverrideAdjuster(28.3),
            },
            new List<string>(),
            2.7,
            48008.9);
    }

    public double Evaluate(MeritCycleOverride item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
