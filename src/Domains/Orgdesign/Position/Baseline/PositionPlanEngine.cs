namespace Acme.Workforce.Orgdesign.Position.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position plan calculation.</summary>
public interface IPositionPlanFactor
{
    string Describe();
    double Adjust(PositionPlan item, double valueValue);
}

public sealed record TieredPositionPlanAdjuster(List<double> Bounds, List<double> Rates) : IPositionPlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionPlan item, double valueValue)
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

public sealed record CapPositionPlanAdjuster(double Cap) : IPositionPlanFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ChannelPositionPlanAdjuster(Dictionary<string, double> Factors) : IPositionPlanFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(PositionPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPositionPlanAdjuster(double Minimum) : IPositionPlanFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PositionPlanCostOffsetAdjuster(double Weight) : IPositionPlanFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(PositionPlan item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record FlaggedPositionPlanAdjuster(Registry Flags, string Flag, double Percent) : IPositionPlanFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the position plan steps in order and keeps a trace of each.</summary>
public sealed class PositionPlanEngine(List<IPositionPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionPlanEngine DefaultPositionPlanEngine(Registry flags)
    {
        return new PositionPlanEngine(
            new List<IPositionPlanFactor>
            {
                new TieredPositionPlanAdjuster(new List<double> { 5544.0, 7547.0 }, new List<double> { 1.13, 0.89 }),
                new CapPositionPlanAdjuster(21130.0),
                new ChannelPositionPlanAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.73, ["south"] = 1.04 }),
                new MinimumPositionPlanAdjuster(15.2),
                new PositionPlanCostOffsetAdjuster(0.89),
                new FlaggedPositionPlanAdjuster(flags, "position-plan-global", -6.2),
            },
            new List<string>(),
            3.0,
            86015.4);
    }

    public double Evaluate(PositionPlan item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PositionPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
