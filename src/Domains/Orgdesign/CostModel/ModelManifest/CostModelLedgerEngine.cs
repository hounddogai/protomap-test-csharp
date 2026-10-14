namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model ledger calculation.</summary>
public interface ICostModelLedgerAdjuster
{
    string Describe();
    double Adjust(CostModelLedger item, double valueValue);
}

public sealed record TieredCostModelLedgerAdjuster(List<double> Bounds, List<double> Rates) : ICostModelLedgerAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelLedger item, double valueValue)
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

public sealed record MinimumCostModelLedgerAdjuster(double Minimum) : ICostModelLedgerAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingCostModelLedgerAdjuster(double Step) : ICostModelLedgerAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentCostModelLedgerAdjuster(double Percent) : ICostModelLedgerAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CostModelLedgerFloorValueOffsetAdjuster(double Weight) : ICostModelLedgerAdjuster
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(CostModelLedger item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record CapCostModelLedgerAdjuster(double Cap) : ICostModelLedgerAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CostModelLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the cost model ledger steps in order and keeps a trace of each.</summary>
public sealed class CostModelLedgerEngine(List<ICostModelLedgerAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelLedgerAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelLedgerEngine DefaultCostModelLedgerEngine()
    {
        return new CostModelLedgerEngine(
            new List<ICostModelLedgerAdjuster>
            {
                new TieredCostModelLedgerAdjuster(
                    new List<double> { 1638.0, 9417.0 },
                    new List<double> { 1.15, 1.06 }),
                new MinimumCostModelLedgerAdjuster(12.7),
                new RoundingCostModelLedgerAdjuster(0.05),
                new PercentCostModelLedgerAdjuster(-14.3),
                new CostModelLedgerFloorValueOffsetAdjuster(0.39),
                new CapCostModelLedgerAdjuster(20016.0),
            },
            new List<string>(),
            2.7,
            39544.3);
    }

    public double Compute(CostModelLedger item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
