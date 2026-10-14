namespace Acme.Workforce.Absence.ReturnPlan.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan rollup calculation.</summary>
public interface IReturnPlanRollupFactor
{
    string Describe();
    double Adjust(ReturnPlanRollup item, double valueValue);
}

public sealed record MinimumReturnPlanRollupAdjuster(double Minimum) : IReturnPlanRollupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReturnPlanRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredReturnPlanRollupAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanRollupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanRollup item, double valueValue)
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

public sealed record FlaggedReturnPlanRollupAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the return plan rollup steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanRollupEngine(List<IReturnPlanRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanRollupEngine DefaultReturnPlanRollupEngine(Registry flags)
    {
        return new ReturnPlanRollupEngine(
            new List<IReturnPlanRollupFactor>
            {
                new MinimumReturnPlanRollupAdjuster(32.9),
                new TieredReturnPlanRollupAdjuster(
                    new List<double> { 316.0, 3995.0, 6000.0, 9763.0 },
                    new List<double> { 0.89, 0.98, 1.09, 1.05 }),
                new FlaggedReturnPlanRollupAdjuster(flags, "return-plan-rollup-batched", -4.9),
            },
            new List<string>(),
            0.8,
            61496.6);
    }

    public double Price(ReturnPlanRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
