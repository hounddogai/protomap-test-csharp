namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped pay run ledger calculation.</summary>
public interface ICappedPayRunLedgerFactor
{
    string Describe();
    double Adjust(CappedPayRunLedger item, double valueValue);
}

public sealed record TieredCappedPayRunLedgerAdjuster(List<double> Bounds, List<double> Rates) : ICappedPayRunLedgerFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedPayRunLedger item, double valueValue)
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

public sealed record PercentCappedPayRunLedgerAdjuster(double Percent) : ICappedPayRunLedgerFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedPayRunLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelCappedPayRunLedgerAdjuster(Dictionary<string, double> Factors) : ICappedPayRunLedgerFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(CappedPayRunLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CappedPayRunLedgerLeaveDaysOffsetAdjuster(double Weight) : ICappedPayRunLedgerFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(CappedPayRunLedger item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record MinimumCappedPayRunLedgerAdjuster(double Minimum) : ICappedPayRunLedgerFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedPayRunLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCappedPayRunLedgerAdjuster(Registry Flags, string Flag, double Percent) : ICappedPayRunLedgerFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedPayRunLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped pay run ledger steps in order and keeps a trace of each.</summary>
public sealed class CappedPayRunLedgerEngine(List<ICappedPayRunLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedPayRunLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedPayRunLedgerEngine DefaultCappedPayRunLedgerEngine(Registry flags)
    {
        return new CappedPayRunLedgerEngine(
            new List<ICappedPayRunLedgerFactor>
            {
                new TieredCappedPayRunLedgerAdjuster(
                    new List<double> { 5944.0, 7728.0 },
                    new List<double> { 0.84, 1.03 }),
                new PercentCappedPayRunLedgerAdjuster(3.8),
                new ChannelCappedPayRunLedgerAdjuster(new Dictionary<string, double> { ["online"] = 0.89 }),
                new CappedPayRunLedgerLeaveDaysOffsetAdjuster(1.08),
                new MinimumCappedPayRunLedgerAdjuster(30.4),
                new FlaggedCappedPayRunLedgerAdjuster(flags, "capped-pay-run-ledger-global", 2.9),
            },
            new List<string>(),
            3.6,
            97742.3);
    }

    public double Evaluate(CappedPayRunLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedPayRunLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
