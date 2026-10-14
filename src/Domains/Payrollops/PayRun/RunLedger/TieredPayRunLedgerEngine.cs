namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered pay run ledger calculation.</summary>
public interface ITieredPayRunLedgerModifier
{
    string Describe();
    double Adjust(TieredPayRunLedger item, double valueValue);
}

public sealed record PercentTieredPayRunLedgerAdjuster(double Percent) : ITieredPayRunLedgerModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredPayRunLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredPayRunLedgerAdjuster(double Cap) : ITieredPayRunLedgerModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredPayRunLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingTieredPayRunLedgerAdjuster(double Step) : ITieredPayRunLedgerModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredPayRunLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tiered pay run ledger steps in order and keeps a trace of each.</summary>
public sealed class TieredPayRunLedgerEngine(List<ITieredPayRunLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredPayRunLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredPayRunLedgerEngine DefaultTieredPayRunLedgerEngine()
    {
        return new TieredPayRunLedgerEngine(
            new List<ITieredPayRunLedgerModifier>
            {
                new PercentTieredPayRunLedgerAdjuster(-17.2),
                new CapTieredPayRunLedgerAdjuster(20740.0),
                new RoundingTieredPayRunLedgerAdjuster(1.0),
            },
            new List<string>(),
            0.4,
            10534.6);
    }

    public double Price(TieredPayRunLedger item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TieredPayRunLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
