namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient merit cycle target calculation.</summary>
public interface ILenientMeritCycleTargetFactor
{
    string Describe();
    double Adjust(LenientMeritCycleTarget item, double valueValue);
}

public sealed record FlaggedLenientMeritCycleTargetAdjuster(Registry Flags, string Flag, double Percent) : ILenientMeritCycleTargetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientMeritCycleTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLenientMeritCycleTargetAdjuster(double Percent) : ILenientMeritCycleTargetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientMeritCycleTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientMeritCycleTargetAdjuster(List<double> Bounds, List<double> Rates) : ILenientMeritCycleTargetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientMeritCycleTarget item, double valueValue)
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

/// <summary>Runs the lenient merit cycle target steps in order and keeps a trace of each.</summary>
public sealed class LenientMeritCycleTargetEngine(List<ILenientMeritCycleTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientMeritCycleTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientMeritCycleTargetEngine DefaultLenientMeritCycleTargetEngine(Registry flags)
    {
        return new LenientMeritCycleTargetEngine(
            new List<ILenientMeritCycleTargetFactor>
            {
                new FlaggedLenientMeritCycleTargetAdjuster(flags, "lenient-merit-cycle-target-legacy", -2.2),
                new PercentLenientMeritCycleTargetAdjuster(13.9),
                new TieredLenientMeritCycleTargetAdjuster(
                    new List<double> { 1112.0, 1827.0, 2084.0, 3841.0 },
                    new List<double> { 0.93, 1.01, 1.17, 0.87 }),
            },
            new List<string>(),
            2.6,
            25713.0);
    }

    public double Evaluate(LenientMeritCycleTarget item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientMeritCycleTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
