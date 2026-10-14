namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict return plan bundle calculation.</summary>
public interface IStrictReturnPlanBundleStep
{
    string Describe();
    double Adjust(StrictReturnPlanBundle item, double valueValue);
}

public sealed record PercentStrictReturnPlanBundleAdjuster(double Percent) : IStrictReturnPlanBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictReturnPlanBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictReturnPlanBundleAdjuster(List<double> Bounds, List<double> Rates) : IStrictReturnPlanBundleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictReturnPlanBundle item, double valueValue)
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

public sealed record CapStrictReturnPlanBundleAdjuster(double Cap) : IStrictReturnPlanBundleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictReturnPlanBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the strict return plan bundle steps in order and keeps a trace of each.</summary>
public sealed class StrictReturnPlanBundleEngine(List<IStrictReturnPlanBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictReturnPlanBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictReturnPlanBundleEngine DefaultStrictReturnPlanBundleEngine()
    {
        return new StrictReturnPlanBundleEngine(
            new List<IStrictReturnPlanBundleStep>
            {
                new PercentStrictReturnPlanBundleAdjuster(20.3),
                new TieredStrictReturnPlanBundleAdjuster(
                    new List<double> { 6394.0, 7997.0, 9946.0 },
                    new List<double> { 0.82, 1.03, 1.06 }),
                new CapStrictReturnPlanBundleAdjuster(17640.0),
            },
            new List<string>(),
            4.8,
            55907.0);
    }

    public double Run(StrictReturnPlanBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictReturnPlanBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
