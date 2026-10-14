namespace Acme.Workforce.Absence.Accrual.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global accrual estimate calculation.</summary>
public interface IGlobalAccrualEstimateAdjuster
{
    string Describe();
    double Adjust(GlobalAccrualEstimate item, double valueValue);
}

public sealed record TieredGlobalAccrualEstimateAdjuster(List<double> Bounds, List<double> Rates) : IGlobalAccrualEstimateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalAccrualEstimate item, double valueValue)
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

public sealed record RoundingGlobalAccrualEstimateAdjuster(double Step) : IGlobalAccrualEstimateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalAccrualEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapGlobalAccrualEstimateAdjuster(double Cap) : IGlobalAccrualEstimateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalAccrualEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record GlobalAccrualEstimateMinimumOffsetAdjuster(double Weight) : IGlobalAccrualEstimateAdjuster
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(GlobalAccrualEstimate item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record NameGlobalAccrualEstimateAdjuster(Dictionary<string, double> Factors) : IGlobalAccrualEstimateAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(GlobalAccrualEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the global accrual estimate steps in order and keeps a trace of each.</summary>
public sealed class GlobalAccrualEstimateEngine(List<IGlobalAccrualEstimateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalAccrualEstimateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalAccrualEstimateEngine DefaultGlobalAccrualEstimateEngine()
    {
        return new GlobalAccrualEstimateEngine(
            new List<IGlobalAccrualEstimateAdjuster>
            {
                new TieredGlobalAccrualEstimateAdjuster(
                    new List<double> { 606.0, 6697.0, 9532.0, 9614.0 },
                    new List<double> { 0.84, 0.86, 0.96, 0.84 }),
                new RoundingGlobalAccrualEstimateAdjuster(5.0),
                new CapGlobalAccrualEstimateAdjuster(14058.0),
                new GlobalAccrualEstimateMinimumOffsetAdjuster(1.35),
                new NameGlobalAccrualEstimateAdjuster(new Dictionary<string, double> { ["retail"] = 0.73 }),
            },
            new List<string>(),
            2.3,
            43722.5);
    }

    public double Evaluate(GlobalAccrualEstimate item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalAccrualEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
