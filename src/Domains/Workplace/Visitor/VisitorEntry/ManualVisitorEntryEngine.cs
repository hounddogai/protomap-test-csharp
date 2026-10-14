namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual visitor entry calculation.</summary>
public interface IManualVisitorEntryModifier
{
    string Describe();
    double Adjust(ManualVisitorEntry item, double valueValue);
}

public sealed record ManualVisitorEntryMinimumOffsetAdjuster(double Weight) : IManualVisitorEntryModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(ManualVisitorEntry item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record CapManualVisitorEntryAdjuster(double Cap) : IManualVisitorEntryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualVisitorEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SegmentManualVisitorEntryAdjuster(Dictionary<string, double> Factors) : IManualVisitorEntryModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(ManualVisitorEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredManualVisitorEntryAdjuster(List<double> Bounds, List<double> Rates) : IManualVisitorEntryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualVisitorEntry item, double valueValue)
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

public sealed record FlaggedManualVisitorEntryAdjuster(Registry Flags, string Flag, double Percent) : IManualVisitorEntryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualVisitorEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual visitor entry steps in order and keeps a trace of each.</summary>
public sealed class ManualVisitorEntryEngine(List<IManualVisitorEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualVisitorEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualVisitorEntryEngine DefaultManualVisitorEntryEngine(Registry flags)
    {
        return new ManualVisitorEntryEngine(
            new List<IManualVisitorEntryModifier>
            {
                new ManualVisitorEntryMinimumOffsetAdjuster(1.87),
                new CapManualVisitorEntryAdjuster(14697.0),
                new SegmentManualVisitorEntryAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.18, ["north"] = 0.84, ["retail"] = 1.03 }),
                new TieredManualVisitorEntryAdjuster(
                    new List<double> { 1588.0, 2007.0, 5451.0 },
                    new List<double> { 0.91, 1.11, 1.06 }),
                new FlaggedManualVisitorEntryAdjuster(flags, "manual-visitor-entry-manual", -8.1),
            },
            new List<string>(),
            1.2,
            36413.2);
    }

    public double Run(ManualVisitorEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualVisitorEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
