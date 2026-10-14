namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional warranty ledger calculation.</summary>
public interface IRegionalWarrantyLedgerAdjuster
{
    string Describe();
    double Adjust(RegionalWarrantyLedger item, double valueValue);
}

public sealed record CapRegionalWarrantyLedgerAdjuster(double Cap) : IRegionalWarrantyLedgerAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalWarrantyLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRegionalWarrantyLedgerAdjuster(double Percent) : IRegionalWarrantyLedgerAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalWarrantyLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRegionalWarrantyLedgerAdjuster(double Step) : IRegionalWarrantyLedgerAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalWarrantyLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CountryRegionalWarrantyLedgerAdjuster(Dictionary<string, double> Factors) : IRegionalWarrantyLedgerAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(RegionalWarrantyLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRegionalWarrantyLedgerAdjuster(List<double> Bounds, List<double> Rates) : IRegionalWarrantyLedgerAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalWarrantyLedger item, double valueValue)
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

/// <summary>Runs the regional warranty ledger steps in order and keeps a trace of each.</summary>
public sealed class RegionalWarrantyLedgerEngine(List<IRegionalWarrantyLedgerAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalWarrantyLedgerAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalWarrantyLedgerEngine DefaultRegionalWarrantyLedgerEngine()
    {
        return new RegionalWarrantyLedgerEngine(
            new List<IRegionalWarrantyLedgerAdjuster>
            {
                new CapRegionalWarrantyLedgerAdjuster(43157.0),
                new PercentRegionalWarrantyLedgerAdjuster(19.4),
                new RoundingRegionalWarrantyLedgerAdjuster(0.1),
                new CountryRegionalWarrantyLedgerAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.06, ["west"] = 0.82 }),
                new TieredRegionalWarrantyLedgerAdjuster(
                    new List<double> { 7606.0, 8507.0, 9440.0 },
                    new List<double> { 0.96, 0.82, 1.07 }),
            },
            new List<string>(),
            0.4,
            85814.4);
    }

    public double Evaluate(RegionalWarrantyLedger item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalWarrantyLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
