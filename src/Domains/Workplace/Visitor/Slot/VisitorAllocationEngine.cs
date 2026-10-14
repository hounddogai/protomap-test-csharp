namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor allocation calculation.</summary>
public interface IVisitorAllocationModifier
{
    string Describe();
    double Adjust(VisitorAllocation item, double valueValue);
}

public sealed record CapVisitorAllocationAdjuster(double Cap) : IVisitorAllocationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredVisitorAllocationAdjuster(List<double> Bounds, List<double> Rates) : IVisitorAllocationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorAllocation item, double valueValue)
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

public sealed record FlaggedVisitorAllocationAdjuster(Registry Flags, string Flag, double Percent) : IVisitorAllocationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the visitor allocation steps in order and keeps a trace of each.</summary>
public sealed class VisitorAllocationEngine(List<IVisitorAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorAllocationEngine DefaultVisitorAllocationEngine(Registry flags)
    {
        return new VisitorAllocationEngine(
            new List<IVisitorAllocationModifier>
            {
                new CapVisitorAllocationAdjuster(18247.0),
                new TieredVisitorAllocationAdjuster(
                    new List<double> { 6982.0, 8007.0 },
                    new List<double> { 1.18, 0.99 }),
                new FlaggedVisitorAllocationAdjuster(flags, "visitor-allocation-primary", -6.6),
            },
            new List<string>(),
            2.5,
            42392.1);
    }

    public double Price(VisitorAllocation item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
