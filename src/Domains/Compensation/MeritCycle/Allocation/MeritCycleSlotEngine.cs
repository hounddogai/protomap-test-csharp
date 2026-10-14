namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle slot calculation.</summary>
public interface IMeritCycleSlotModifier
{
    string Describe();
    double Adjust(MeritCycleSlot item, double valueValue);
}

public sealed record CapMeritCycleSlotAdjuster(double Cap) : IMeritCycleSlotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCycleSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MeritCycleSlotRevisionOffsetAdjuster(double Weight) : IMeritCycleSlotModifier
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(MeritCycleSlot item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record FlaggedMeritCycleSlotAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleSlotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingMeritCycleSlotAdjuster(double Step) : IMeritCycleSlotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MeritCycleSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitMeritCycleSlotAdjuster(Dictionary<string, double> Factors) : IMeritCycleSlotModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(MeritCycleSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumMeritCycleSlotAdjuster(double Minimum) : IMeritCycleSlotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCycleSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the merit cycle slot steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleSlotEngine(List<IMeritCycleSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleSlotEngine DefaultMeritCycleSlotEngine(Registry flags)
    {
        return new MeritCycleSlotEngine(
            new List<IMeritCycleSlotModifier>
            {
                new CapMeritCycleSlotAdjuster(43766.0),
                new MeritCycleSlotRevisionOffsetAdjuster(1.15),
                new FlaggedMeritCycleSlotAdjuster(flags, "merit-cycle-slot-lenient", 2.6),
                new RoundingMeritCycleSlotAdjuster(1.0),
                new UnitMeritCycleSlotAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.88, ["central"] = 0.81, ["east"] = 0.81 }),
                new MinimumMeritCycleSlotAdjuster(41.0),
            },
            new List<string>(),
            2.7,
            83726.3);
    }

    public double Run(MeritCycleSlot item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
