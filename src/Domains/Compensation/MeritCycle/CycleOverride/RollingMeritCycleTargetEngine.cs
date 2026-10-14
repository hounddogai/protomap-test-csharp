namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling merit cycle target calculation.</summary>
public interface IRollingMeritCycleTargetModifier
{
    string Describe();
    double Adjust(RollingMeritCycleTarget item, double valueValue);
}

public sealed record TieredRollingMeritCycleTargetAdjuster(List<double> Bounds, List<double> Rates) : IRollingMeritCycleTargetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingMeritCycleTarget item, double valueValue)
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

public sealed record PercentRollingMeritCycleTargetAdjuster(double Percent) : IRollingMeritCycleTargetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingMeritCycleTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleRollingMeritCycleTargetAdjuster(Dictionary<string, double> Factors) : IRollingMeritCycleTargetModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RollingMeritCycleTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapRollingMeritCycleTargetAdjuster(double Cap) : IRollingMeritCycleTargetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingMeritCycleTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRollingMeritCycleTargetAdjuster(double Step) : IRollingMeritCycleTargetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingMeritCycleTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the rolling merit cycle target steps in order and keeps a trace of each.</summary>
public sealed class RollingMeritCycleTargetEngine(List<IRollingMeritCycleTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingMeritCycleTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingMeritCycleTargetEngine DefaultRollingMeritCycleTargetEngine()
    {
        return new RollingMeritCycleTargetEngine(
            new List<IRollingMeritCycleTargetModifier>
            {
                new TieredRollingMeritCycleTargetAdjuster(
                    new List<double> { 2148.0, 6897.0, 8183.0 },
                    new List<double> { 1.12, 0.86, 0.88 }),
                new PercentRollingMeritCycleTargetAdjuster(-24.2),
                new TitleRollingMeritCycleTargetAdjuster(new Dictionary<string, double> { ["online"] = 1.22 }),
                new CapRollingMeritCycleTargetAdjuster(26539.0),
                new RoundingRollingMeritCycleTargetAdjuster(5.0),
            },
            new List<string>(),
            3.8,
            35549.7);
    }

    public double Evaluate(RollingMeritCycleTarget item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RollingMeritCycleTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
