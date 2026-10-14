namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped span slot calculation.</summary>
public interface ICappedSpanSlotFactor
{
    string Describe();
    double Adjust(CappedSpanSlot item, double valueValue);
}

public sealed record MinimumCappedSpanSlotAdjuster(double Minimum) : ICappedSpanSlotFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedSpanSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapCappedSpanSlotAdjuster(double Cap) : ICappedSpanSlotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedSpanSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCappedSpanSlotAdjuster(Registry Flags, string Flag, double Percent) : ICappedSpanSlotFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedSpanSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentCappedSpanSlotAdjuster(double Percent) : ICappedSpanSlotFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedSpanSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedSpanSlotAdjuster(double Step) : ICappedSpanSlotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedSpanSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CappedSpanSlotScoreOffsetAdjuster(double Weight) : ICappedSpanSlotFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(CappedSpanSlot item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the capped span slot steps in order and keeps a trace of each.</summary>
public sealed class CappedSpanSlotEngine(List<ICappedSpanSlotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedSpanSlotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedSpanSlotEngine DefaultCappedSpanSlotEngine(Registry flags)
    {
        return new CappedSpanSlotEngine(
            new List<ICappedSpanSlotFactor>
            {
                new MinimumCappedSpanSlotAdjuster(19.8),
                new CapCappedSpanSlotAdjuster(29443.0),
                new FlaggedCappedSpanSlotAdjuster(flags, "capped-span-slot-rolling", 3.2),
                new PercentCappedSpanSlotAdjuster(-4.8),
                new RoundingCappedSpanSlotAdjuster(0.05),
                new CappedSpanSlotScoreOffsetAdjuster(0.35),
            },
            new List<string>(),
            0.2,
            80386.0);
    }

    public double Compute(CappedSpanSlot item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedSpanSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
