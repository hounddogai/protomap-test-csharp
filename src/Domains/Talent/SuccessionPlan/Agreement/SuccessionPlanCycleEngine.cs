namespace Acme.Workforce.Talent.SuccessionPlan.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan cycle calculation.</summary>
public interface ISuccessionPlanCycleFactor
{
    string Describe();
    double Adjust(SuccessionPlanCycle item, double valueValue);
}

public sealed record CapSuccessionPlanCycleAdjuster(double Cap) : ISuccessionPlanCycleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedSuccessionPlanCycleAdjuster(Registry Flags, string Flag, double Percent) : ISuccessionPlanCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SuccessionPlanCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSuccessionPlanCycleAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanCycleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanCycle item, double valueValue)
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

public sealed record RoundingSuccessionPlanCycleAdjuster(double Step) : ISuccessionPlanCycleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSuccessionPlanCycleAdjuster(double Minimum) : ISuccessionPlanCycleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the succession plan cycle steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanCycleEngine(List<ISuccessionPlanCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanCycleEngine DefaultSuccessionPlanCycleEngine(Registry flags)
    {
        return new SuccessionPlanCycleEngine(
            new List<ISuccessionPlanCycleFactor>
            {
                new CapSuccessionPlanCycleAdjuster(24308.0),
                new FlaggedSuccessionPlanCycleAdjuster(flags, "succession-plan-cycle-scheduled", -0.2),
                new TieredSuccessionPlanCycleAdjuster(
                    new List<double> { 1750.0, 3171.0, 9679.0, 9996.0 },
                    new List<double> { 1.17, 0.83, 1.08, 0.86 }),
                new RoundingSuccessionPlanCycleAdjuster(0.5),
                new MinimumSuccessionPlanCycleAdjuster(20.1),
            },
            new List<string>(),
            1.4,
            65596.2);
    }

    public double Evaluate(SuccessionPlanCycle item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
