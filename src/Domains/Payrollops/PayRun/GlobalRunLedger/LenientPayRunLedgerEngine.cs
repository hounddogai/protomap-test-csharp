namespace Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient pay run ledger calculation.</summary>
public interface ILenientPayRunLedgerAdjuster
{
    string Describe();
    double Adjust(LenientPayRunLedger item, double valueValue);
}

public sealed record TieredLenientPayRunLedgerAdjuster(List<double> Bounds, List<double> Rates) : ILenientPayRunLedgerAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientPayRunLedger item, double valueValue)
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

public sealed record FlaggedLenientPayRunLedgerAdjuster(Registry Flags, string Flag, double Percent) : ILenientPayRunLedgerAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientPayRunLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientPayRunLedgerAdjuster(double Minimum) : ILenientPayRunLedgerAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientPayRunLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLenientPayRunLedgerAdjuster(double Cap) : ILenientPayRunLedgerAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientPayRunLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the lenient pay run ledger steps in order and keeps a trace of each.</summary>
public sealed class LenientPayRunLedgerEngine(List<ILenientPayRunLedgerAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientPayRunLedgerAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientPayRunLedgerEngine DefaultLenientPayRunLedgerEngine(Registry flags)
    {
        return new LenientPayRunLedgerEngine(
            new List<ILenientPayRunLedgerAdjuster>
            {
                new TieredLenientPayRunLedgerAdjuster(
                    new List<double> { 5444.0, 5605.0 },
                    new List<double> { 0.82, 1.03 }),
                new FlaggedLenientPayRunLedgerAdjuster(flags, "lenient-pay-run-ledger-pending", 2.1),
                new MinimumLenientPayRunLedgerAdjuster(14.3),
                new CapLenientPayRunLedgerAdjuster(21564.0),
            },
            new List<string>(),
            2.7,
            24362.7);
    }

    public double Price(LenientPayRunLedger item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LenientPayRunLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
