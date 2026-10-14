namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band target calculation.</summary>
public interface IPayBandTargetStep
{
    string Describe();
    double Adjust(PayBandTarget item, double valueValue);
}

public sealed record TieredPayBandTargetAdjuster(List<double> Bounds, List<double> Rates) : IPayBandTargetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandTarget item, double valueValue)
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

public sealed record NamePayBandTargetAdjuster(Dictionary<string, double> Factors) : IPayBandTargetStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PayBandTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPayBandTargetAdjuster(double Minimum) : IPayBandTargetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pay band target steps in order and keeps a trace of each.</summary>
public sealed class PayBandTargetEngine(List<IPayBandTargetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandTargetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandTargetEngine DefaultPayBandTargetEngine()
    {
        return new PayBandTargetEngine(
            new List<IPayBandTargetStep>
            {
                new TieredPayBandTargetAdjuster(
                    new List<double> { 2823.0, 3261.0, 3423.0 },
                    new List<double> { 0.98, 0.91, 1.19 }),
                new NamePayBandTargetAdjuster(new Dictionary<string, double> { ["central"] = 1.11 }),
                new MinimumPayBandTargetAdjuster(49.2),
            },
            new List<string>(),
            0.4,
            17580.0);
    }

    public double Run(PayBandTarget item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
