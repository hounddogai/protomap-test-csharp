namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted merit cycle claim calculation.</summary>
public interface IWeightedMeritCycleClaimFactor
{
    string Describe();
    double Adjust(WeightedMeritCycleClaim item, double valueValue);
}

public sealed record PercentWeightedMeritCycleClaimAdjuster(double Percent) : IWeightedMeritCycleClaimFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedMeritCycleClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryWeightedMeritCycleClaimAdjuster(Dictionary<string, double> Factors) : IWeightedMeritCycleClaimFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(WeightedMeritCycleClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingWeightedMeritCycleClaimAdjuster(double Step) : IWeightedMeritCycleClaimFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedMeritCycleClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the weighted merit cycle claim steps in order and keeps a trace of each.</summary>
public sealed class WeightedMeritCycleClaimEngine(List<IWeightedMeritCycleClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedMeritCycleClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedMeritCycleClaimEngine DefaultWeightedMeritCycleClaimEngine()
    {
        return new WeightedMeritCycleClaimEngine(
            new List<IWeightedMeritCycleClaimFactor>
            {
                new PercentWeightedMeritCycleClaimAdjuster(9.6),
                new CategoryWeightedMeritCycleClaimAdjuster(new Dictionary<string, double> { ["east"] = 0.98 }),
                new RoundingWeightedMeritCycleClaimAdjuster(0.25),
            },
            new List<string>(),
            0.4,
            61048.4);
    }

    public double Compute(WeightedMeritCycleClaim item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedMeritCycleClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
