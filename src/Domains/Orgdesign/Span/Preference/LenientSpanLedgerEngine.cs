namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient span ledger calculation.</summary>
public interface ILenientSpanLedgerModifier
{
    string Describe();
    double Adjust(LenientSpanLedger item, double valueValue);
}

public sealed record LabelLenientSpanLedgerAdjuster(Dictionary<string, double> Factors) : ILenientSpanLedgerModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LenientSpanLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLenientSpanLedgerAdjuster(Registry Flags, string Flag, double Percent) : ILenientSpanLedgerModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientSpanLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLenientSpanLedgerAdjuster(double Cap) : ILenientSpanLedgerModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientSpanLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLenientSpanLedgerAdjuster(List<double> Bounds, List<double> Rates) : ILenientSpanLedgerModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientSpanLedger item, double valueValue)
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

public sealed record LenientSpanLedgerUpdatedAtOffsetAdjuster(double Weight) : ILenientSpanLedgerModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(LenientSpanLedger item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

/// <summary>Runs the lenient span ledger steps in order and keeps a trace of each.</summary>
public sealed class LenientSpanLedgerEngine(List<ILenientSpanLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientSpanLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientSpanLedgerEngine DefaultLenientSpanLedgerEngine(Registry flags)
    {
        return new LenientSpanLedgerEngine(
            new List<ILenientSpanLedgerModifier>
            {
                new LabelLenientSpanLedgerAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.1, ["online"] = 1.37, ["central"] = 0.79 }),
                new FlaggedLenientSpanLedgerAdjuster(flags, "lenient-span-ledger-tiered", 7.1),
                new CapLenientSpanLedgerAdjuster(15405.0),
                new TieredLenientSpanLedgerAdjuster(
                    new List<double> { 551.0, 4216.0, 7788.0 },
                    new List<double> { 1.17, 1.11, 0.85 }),
                new LenientSpanLedgerUpdatedAtOffsetAdjuster(0.03),
            },
            new List<string>(),
            4.6,
            83288.0);
    }

    public double Compute(LenientSpanLedger item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LenientSpanLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
