namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted merit cycle tier calculation.</summary>
public interface IWeightedMeritCycleTierStep
{
    string Describe();
    double Adjust(WeightedMeritCycleTier item, double valueValue);
}

public sealed record PercentWeightedMeritCycleTierAdjuster(double Percent) : IWeightedMeritCycleTierStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedMeritCycleTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWeightedMeritCycleTierAdjuster(double Step) : IWeightedMeritCycleTierStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedMeritCycleTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelWeightedMeritCycleTierAdjuster(Dictionary<string, double> Factors) : IWeightedMeritCycleTierStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(WeightedMeritCycleTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapWeightedMeritCycleTierAdjuster(double Cap) : IWeightedMeritCycleTierStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedMeritCycleTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredWeightedMeritCycleTierAdjuster(List<double> Bounds, List<double> Rates) : IWeightedMeritCycleTierStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedMeritCycleTier item, double valueValue)
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

/// <summary>Runs the weighted merit cycle tier steps in order and keeps a trace of each.</summary>
public sealed class WeightedMeritCycleTierEngine(List<IWeightedMeritCycleTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedMeritCycleTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedMeritCycleTierEngine DefaultWeightedMeritCycleTierEngine()
    {
        return new WeightedMeritCycleTierEngine(
            new List<IWeightedMeritCycleTierStep>
            {
                new PercentWeightedMeritCycleTierAdjuster(13.8),
                new RoundingWeightedMeritCycleTierAdjuster(0.1),
                new LabelWeightedMeritCycleTierAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.99, ["south"] = 1.08 }),
                new CapWeightedMeritCycleTierAdjuster(3828.0),
                new TieredWeightedMeritCycleTierAdjuster(
                    new List<double> { 2656.0, 9499.0 },
                    new List<double> { 1.13, 0.92 }),
            },
            new List<string>(),
            2.8,
            19536.1);
    }

    public double Compute(WeightedMeritCycleTier item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedMeritCycleTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
