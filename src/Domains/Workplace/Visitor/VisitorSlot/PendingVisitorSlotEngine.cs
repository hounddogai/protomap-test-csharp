namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending visitor slot calculation.</summary>
public interface IPendingVisitorSlotAdjuster
{
    string Describe();
    double Adjust(PendingVisitorSlot item, double valueValue);
}

public sealed record FlaggedPendingVisitorSlotAdjuster(Registry Flags, string Flag, double Percent) : IPendingVisitorSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingVisitorSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPendingVisitorSlotAdjuster(double Minimum) : IPendingVisitorSlotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingVisitorSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PendingVisitorSlotGraceDaysOffsetAdjuster(double Weight) : IPendingVisitorSlotAdjuster
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(PendingVisitorSlot item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record TieredPendingVisitorSlotAdjuster(List<double> Bounds, List<double> Rates) : IPendingVisitorSlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingVisitorSlot item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

/// <summary>Runs the pending visitor slot steps in order and keeps a trace of each.</summary>
public sealed class PendingVisitorSlotEngine(List<IPendingVisitorSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingVisitorSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingVisitorSlotEngine DefaultPendingVisitorSlotEngine(Registry flags)
    {
        return new PendingVisitorSlotEngine(
            new List<IPendingVisitorSlotAdjuster>
            {
                new FlaggedPendingVisitorSlotAdjuster(flags, "pending-visitor-slot-tiered", 6.6),
                new MinimumPendingVisitorSlotAdjuster(1.8),
                new PendingVisitorSlotGraceDaysOffsetAdjuster(1.12),
                new TieredPendingVisitorSlotAdjuster(
                    new List<double> { 656.0, 7956.0 },
                    new List<double> { 0.99, 1.1 }),
            },
            new List<string>(),
            3.7,
            68561.8);
    }

    public double Evaluate(PendingVisitorSlot item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingVisitorSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
