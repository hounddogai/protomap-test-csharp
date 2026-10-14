namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling merit cycle override calculation.</summary>
public interface IRollingMeritCycleOverrideStep
{
    string Describe();
    double Adjust(RollingMeritCycleOverride item, double valueValue);
}

public sealed record FlaggedRollingMeritCycleOverrideAdjuster(Registry Flags, string Flag, double Percent) : IRollingMeritCycleOverrideStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingMeritCycleOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRollingMeritCycleOverrideAdjuster(double Step) : IRollingMeritCycleOverrideStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingMeritCycleOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RollingMeritCycleOverrideStartsAtOffsetAdjuster(double Weight) : IRollingMeritCycleOverrideStep
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(RollingMeritCycleOverride item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record TieredRollingMeritCycleOverrideAdjuster(List<double> Bounds, List<double> Rates) : IRollingMeritCycleOverrideStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingMeritCycleOverride item, double valueValue)
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

public sealed record CapRollingMeritCycleOverrideAdjuster(double Cap) : IRollingMeritCycleOverrideStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingMeritCycleOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameRollingMeritCycleOverrideAdjuster(Dictionary<string, double> Factors) : IRollingMeritCycleOverrideStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RollingMeritCycleOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the rolling merit cycle override steps in order and keeps a trace of each.</summary>
public sealed class RollingMeritCycleOverrideEngine(List<IRollingMeritCycleOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingMeritCycleOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingMeritCycleOverrideEngine DefaultRollingMeritCycleOverrideEngine(Registry flags)
    {
        return new RollingMeritCycleOverrideEngine(
            new List<IRollingMeritCycleOverrideStep>
            {
                new FlaggedRollingMeritCycleOverrideAdjuster(flags, "rolling-merit-cycle-override-rolling", -0.0),
                new RoundingRollingMeritCycleOverrideAdjuster(0.1),
                new RollingMeritCycleOverrideStartsAtOffsetAdjuster(0.76),
                new TieredRollingMeritCycleOverrideAdjuster(
                    new List<double> { 1194.0, 1482.0, 3763.0, 3803.0 },
                    new List<double> { 1.0, 1.06, 0.84, 1.02 }),
                new CapRollingMeritCycleOverrideAdjuster(49148.0),
                new NameRollingMeritCycleOverrideAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.93, ["north"] = 0.71, ["online"] = 0.96 }),
            },
            new List<string>(),
            4.5,
            56667.0);
    }

    public double Evaluate(RollingMeritCycleOverride item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RollingMeritCycleOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
