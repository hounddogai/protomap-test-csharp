namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback desk ledger calculation.</summary>
public interface IFallbackDeskLedgerAdjuster
{
    string Describe();
    double Adjust(FallbackDeskLedger item, double valueValue);
}

public sealed record PercentFallbackDeskLedgerAdjuster(double Percent) : IFallbackDeskLedgerAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackDeskLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredFallbackDeskLedgerAdjuster(List<double> Bounds, List<double> Rates) : IFallbackDeskLedgerAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackDeskLedger item, double valueValue)
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

public sealed record RoundingFallbackDeskLedgerAdjuster(double Step) : IFallbackDeskLedgerAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackDeskLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapFallbackDeskLedgerAdjuster(double Cap) : IFallbackDeskLedgerAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackDeskLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FallbackDeskLedgerBatchSizeOffsetAdjuster(double Weight) : IFallbackDeskLedgerAdjuster
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(FallbackDeskLedger item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

/// <summary>Runs the fallback desk ledger steps in order and keeps a trace of each.</summary>
public sealed class FallbackDeskLedgerEngine(List<IFallbackDeskLedgerAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackDeskLedgerAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackDeskLedgerEngine DefaultFallbackDeskLedgerEngine()
    {
        return new FallbackDeskLedgerEngine(
            new List<IFallbackDeskLedgerAdjuster>
            {
                new PercentFallbackDeskLedgerAdjuster(17.3),
                new TieredFallbackDeskLedgerAdjuster(
                    new List<double> { 24.0, 3321.0, 5654.0 },
                    new List<double> { 1.06, 1.07, 1.0 }),
                new RoundingFallbackDeskLedgerAdjuster(0.5),
                new CapFallbackDeskLedgerAdjuster(11823.0),
                new FallbackDeskLedgerBatchSizeOffsetAdjuster(1.53),
            },
            new List<string>(),
            1.3,
            40914.1);
    }

    public double Price(FallbackDeskLedger item)
    {
        var current = item.WeightedValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<FallbackDeskLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
