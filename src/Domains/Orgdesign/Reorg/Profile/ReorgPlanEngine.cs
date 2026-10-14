namespace Acme.Workforce.Orgdesign.Reorg.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg plan calculation.</summary>
public interface IReorgPlanModifier
{
    string Describe();
    double Adjust(ReorgPlan item, double valueValue);
}

public sealed record CapReorgPlanAdjuster(double Cap) : IReorgPlanModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReorgPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingReorgPlanAdjuster(double Step) : IReorgPlanModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredReorgPlanAdjuster(List<double> Bounds, List<double> Rates) : IReorgPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgPlan item, double valueValue)
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

public sealed record PercentReorgPlanAdjuster(double Percent) : IReorgPlanModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReorgPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the reorg plan steps in order and keeps a trace of each.</summary>
public sealed class ReorgPlanEngine(List<IReorgPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgPlanEngine DefaultReorgPlanEngine()
    {
        return new ReorgPlanEngine(
            new List<IReorgPlanModifier>
            {
                new CapReorgPlanAdjuster(6883.0),
                new RoundingReorgPlanAdjuster(0.5),
                new TieredReorgPlanAdjuster(new List<double> { 271.0, 9875.0 }, new List<double> { 1.19, 1.03 }),
                new PercentReorgPlanAdjuster(-9.5),
            },
            new List<string>(),
            3.3,
            92868.0);
    }

    public double Run(ReorgPlan item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
