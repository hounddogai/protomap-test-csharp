namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback pay run cycle calculation.</summary>
public interface IFallbackPayRunCycleFactor
{
    string Describe();
    double Adjust(FallbackPayRunCycle item, double valueValue);
}

public sealed record FlaggedFallbackPayRunCycleAdjuster(Registry Flags, string Flag, double Percent) : IFallbackPayRunCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackPayRunCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFallbackPayRunCycleAdjuster(double Minimum) : IFallbackPayRunCycleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackPayRunCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FallbackPayRunCycleHoursOffsetAdjuster(double Weight) : IFallbackPayRunCycleFactor
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(FallbackPayRunCycle item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

/// <summary>Runs the fallback pay run cycle steps in order and keeps a trace of each.</summary>
public sealed class FallbackPayRunCycleEngine(List<IFallbackPayRunCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPayRunCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPayRunCycleEngine DefaultFallbackPayRunCycleEngine(Registry flags)
    {
        return new FallbackPayRunCycleEngine(
            new List<IFallbackPayRunCycleFactor>
            {
                new FlaggedFallbackPayRunCycleAdjuster(flags, "fallback-pay-run-cycle-scheduled", -3.7),
                new MinimumFallbackPayRunCycleAdjuster(15.7),
                new FallbackPayRunCycleHoursOffsetAdjuster(1.78),
            },
            new List<string>(),
            1.9,
            39200.6);
    }

    public double Evaluate(FallbackPayRunCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPayRunCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
