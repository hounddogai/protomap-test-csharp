namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle plan calculation.</summary>
public interface IMeritCyclePlanModifier
{
    string Describe();
    double Adjust(MeritCyclePlan item, double valueValue);
}

public sealed record CapMeritCyclePlanAdjuster(double Cap) : IMeritCyclePlanModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCyclePlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingMeritCyclePlanAdjuster(double Step) : IMeritCyclePlanModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MeritCyclePlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredMeritCyclePlanAdjuster(List<double> Bounds, List<double> Rates) : IMeritCyclePlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCyclePlan item, double valueValue)
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

public sealed record MinimumMeritCyclePlanAdjuster(double Minimum) : IMeritCyclePlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCyclePlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the merit cycle plan steps in order and keeps a trace of each.</summary>
public sealed class MeritCyclePlanEngine(List<IMeritCyclePlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCyclePlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCyclePlanEngine DefaultMeritCyclePlanEngine()
    {
        return new MeritCyclePlanEngine(
            new List<IMeritCyclePlanModifier>
            {
                new CapMeritCyclePlanAdjuster(47696.0),
                new RoundingMeritCyclePlanAdjuster(0.5),
                new TieredMeritCyclePlanAdjuster(
                    new List<double> { 1152.0, 2909.0, 2913.0, 5088.0 },
                    new List<double> { 0.9, 1.06, 0.99, 0.92 }),
                new MinimumMeritCyclePlanAdjuster(32.2),
            },
            new List<string>(),
            0.8,
            49889.8);
    }

    public double Compute(MeritCyclePlan item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCyclePlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
