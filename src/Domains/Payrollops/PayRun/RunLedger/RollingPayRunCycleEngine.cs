namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling pay run cycle calculation.</summary>
public interface IRollingPayRunCycleFactor
{
    string Describe();
    double Adjust(RollingPayRunCycle item, double valueValue);
}

public sealed record CapRollingPayRunCycleAdjuster(double Cap) : IRollingPayRunCycleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingPayRunCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelRollingPayRunCycleAdjuster(Dictionary<string, double> Factors) : IRollingPayRunCycleFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RollingPayRunCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRollingPayRunCycleAdjuster(double Percent) : IRollingPayRunCycleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingPayRunCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRollingPayRunCycleAdjuster(Registry Flags, string Flag, double Percent) : IRollingPayRunCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingPayRunCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRollingPayRunCycleAdjuster(double Step) : IRollingPayRunCycleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingPayRunCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the rolling pay run cycle steps in order and keeps a trace of each.</summary>
public sealed class RollingPayRunCycleEngine(List<IRollingPayRunCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingPayRunCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingPayRunCycleEngine DefaultRollingPayRunCycleEngine(Registry flags)
    {
        return new RollingPayRunCycleEngine(
            new List<IRollingPayRunCycleFactor>
            {
                new CapRollingPayRunCycleAdjuster(22807.0),
                new LabelRollingPayRunCycleAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.77, ["north"] = 0.92, ["south"] = 1.39 }),
                new PercentRollingPayRunCycleAdjuster(-6.1),
                new FlaggedRollingPayRunCycleAdjuster(flags, "rolling-pay-run-cycle-adaptive", -1.2),
                new RoundingRollingPayRunCycleAdjuster(0.25),
            },
            new List<string>(),
            0.7,
            17735.3);
    }

    public double Evaluate(RollingPayRunCycle item)
    {
        var current = item.ProjectedValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<RollingPayRunCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
