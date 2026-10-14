namespace Acme.Workforce.Compensation.MeritCycle.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict merit cycle plan calculation.</summary>
public interface IStrictMeritCyclePlanStep
{
    string Describe();
    double Adjust(StrictMeritCyclePlan item, double valueValue);
}

public sealed record RoundingStrictMeritCyclePlanAdjuster(double Step) : IStrictMeritCyclePlanStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictMeritCyclePlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ChannelStrictMeritCyclePlanAdjuster(Dictionary<string, double> Factors) : IStrictMeritCyclePlanStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(StrictMeritCyclePlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredStrictMeritCyclePlanAdjuster(List<double> Bounds, List<double> Rates) : IStrictMeritCyclePlanStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictMeritCyclePlan item, double valueValue)
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

public sealed record MinimumStrictMeritCyclePlanAdjuster(double Minimum) : IStrictMeritCyclePlanStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictMeritCyclePlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the strict merit cycle plan steps in order and keeps a trace of each.</summary>
public sealed class StrictMeritCyclePlanEngine(List<IStrictMeritCyclePlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictMeritCyclePlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictMeritCyclePlanEngine DefaultStrictMeritCyclePlanEngine()
    {
        return new StrictMeritCyclePlanEngine(
            new List<IStrictMeritCyclePlanStep>
            {
                new RoundingStrictMeritCyclePlanAdjuster(0.05),
                new ChannelStrictMeritCyclePlanAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.21, ["north"] = 0.89, ["central"] = 1.32 }),
                new TieredStrictMeritCyclePlanAdjuster(
                    new List<double> { 2456.0, 5129.0 },
                    new List<double> { 0.98, 0.85 }),
                new MinimumStrictMeritCyclePlanAdjuster(36.6),
            },
            new List<string>(),
            3.1,
            4863.2);
    }

    public double Run(StrictMeritCyclePlan item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictMeritCyclePlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
