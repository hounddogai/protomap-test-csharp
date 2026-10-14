namespace Acme.Workforce.Orgdesign.Span.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span ledger calculation.</summary>
public interface ISpanLedgerModifier
{
    string Describe();
    double Adjust(SpanLedger item, double valueValue);
}

public sealed record PercentSpanLedgerAdjuster(double Percent) : ISpanLedgerModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedSpanLedgerAdjuster(Registry Flags, string Flag, double Percent) : ISpanLedgerModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSpanLedgerAdjuster(double Step) : ISpanLedgerModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SpanLedgerCapacityOffsetAdjuster(double Weight) : ISpanLedgerModifier
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(SpanLedger item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

/// <summary>Runs the span ledger steps in order and keeps a trace of each.</summary>
public sealed class SpanLedgerEngine(List<ISpanLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanLedgerEngine DefaultSpanLedgerEngine(Registry flags)
    {
        return new SpanLedgerEngine(
            new List<ISpanLedgerModifier>
            {
                new PercentSpanLedgerAdjuster(-21.7),
                new FlaggedSpanLedgerAdjuster(flags, "span-ledger-pending", -3.2),
                new RoundingSpanLedgerAdjuster(5.0),
                new SpanLedgerCapacityOffsetAdjuster(1.13),
            },
            new List<string>(),
            1.9,
            57232.7);
    }

    public double Price(SpanLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<SpanLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
