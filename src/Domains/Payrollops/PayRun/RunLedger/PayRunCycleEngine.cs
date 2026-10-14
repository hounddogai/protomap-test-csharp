namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run cycle calculation.</summary>
public interface IPayRunCycleAdjuster
{
    string Describe();
    double Adjust(PayRunCycle item, double valueValue);
}

public sealed record RoundingPayRunCycleAdjuster(double Step) : IPayRunCycleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayRunCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PayRunCycleRetriesOffsetAdjuster(double Weight) : IPayRunCycleAdjuster
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(PayRunCycle item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record CapPayRunCycleAdjuster(double Cap) : IPayRunCycleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayRunCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPayRunCycleAdjuster(List<double> Bounds, List<double> Rates) : IPayRunCycleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayRunCycle item, double valueValue)
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

/// <summary>Runs the pay run cycle steps in order and keeps a trace of each.</summary>
public sealed class PayRunCycleEngine(List<IPayRunCycleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunCycleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunCycleEngine DefaultPayRunCycleEngine()
    {
        return new PayRunCycleEngine(
            new List<IPayRunCycleAdjuster>
            {
                new RoundingPayRunCycleAdjuster(0.05),
                new PayRunCycleRetriesOffsetAdjuster(1.27),
                new CapPayRunCycleAdjuster(27757.0),
                new TieredPayRunCycleAdjuster(new List<double> { 1073.0, 2603.0 }, new List<double> { 1.19, 1.11 }),
            },
            new List<string>(),
            1.0,
            37894.8);
    }

    public double Compute(PayRunCycle item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PayRunCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
