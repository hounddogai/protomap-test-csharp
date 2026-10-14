namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the retro pay plan calculation.</summary>
public interface IRetroPayPlanFactor
{
    string Describe();
    double Adjust(RetroPayPlan item, double valueValue);
}

public sealed record TieredRetroPayPlanAdjuster(List<double> Bounds, List<double> Rates) : IRetroPayPlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RetroPayPlan item, double valueValue)
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

public sealed record PercentRetroPayPlanAdjuster(double Percent) : IRetroPayPlanFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RetroPayPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRetroPayPlanAdjuster(double Step) : IRetroPayPlanFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RetroPayPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RetroPayPlanCostOffsetAdjuster(double Weight) : IRetroPayPlanFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(RetroPayPlan item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

/// <summary>Runs the retro pay plan steps in order and keeps a trace of each.</summary>
public sealed class RetroPayPlanEngine(List<IRetroPayPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayPlanEngine DefaultRetroPayPlanEngine()
    {
        return new RetroPayPlanEngine(
            new List<IRetroPayPlanFactor>
            {
                new TieredRetroPayPlanAdjuster(
                    new List<double> { 1126.0, 4925.0, 7098.0, 7656.0 },
                    new List<double> { 1.02, 0.9, 0.86, 0.94 }),
                new PercentRetroPayPlanAdjuster(23.5),
                new RoundingRetroPayPlanAdjuster(0.1),
                new RetroPayPlanCostOffsetAdjuster(1.66),
            },
            new List<string>(),
            0.5,
            79918.6);
    }

    public double Price(RetroPayPlan item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
