namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run ledger calculation.</summary>
public interface IPayRunLedgerModifier
{
    string Describe();
    double Adjust(PayRunLedger item, double valueValue);
}

public sealed record TieredPayRunLedgerAdjuster(List<double> Bounds, List<double> Rates) : IPayRunLedgerModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayRunLedger item, double valueValue)
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

public sealed record SourcePayRunLedgerAdjuster(Dictionary<string, double> Factors) : IPayRunLedgerModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(PayRunLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPayRunLedgerAdjuster(Registry Flags, string Flag, double Percent) : IPayRunLedgerModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayRunLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPayRunLedgerAdjuster(double Cap) : IPayRunLedgerModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayRunLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pay run ledger steps in order and keeps a trace of each.</summary>
public sealed class PayRunLedgerEngine(List<IPayRunLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunLedgerEngine DefaultPayRunLedgerEngine(Registry flags)
    {
        return new PayRunLedgerEngine(
            new List<IPayRunLedgerModifier>
            {
                new TieredPayRunLedgerAdjuster(
                    new List<double> { 4926.0, 6270.0, 7537.0 },
                    new List<double> { 0.85, 1.12, 1.13 }),
                new SourcePayRunLedgerAdjuster(new Dictionary<string, double> { ["online"] = 0.84, ["west"] = 1.26 }),
                new FlaggedPayRunLedgerAdjuster(flags, "pay-run-ledger-strict", 8.1),
                new CapPayRunLedgerAdjuster(26543.0),
            },
            new List<string>(),
            4.8,
            95805.0);
    }

    public double Compute(PayRunLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<PayRunLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
