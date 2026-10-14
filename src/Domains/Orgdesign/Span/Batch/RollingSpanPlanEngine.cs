namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling span plan calculation.</summary>
public interface IRollingSpanPlanAdjuster
{
    string Describe();
    double Adjust(RollingSpanPlan item, double valueValue);
}

public sealed record RollingSpanPlanScoreOffsetAdjuster(double Weight) : IRollingSpanPlanAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(RollingSpanPlan item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record FlaggedRollingSpanPlanAdjuster(Registry Flags, string Flag, double Percent) : IRollingSpanPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingSpanPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRollingSpanPlanAdjuster(double Percent) : IRollingSpanPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingSpanPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameRollingSpanPlanAdjuster(Dictionary<string, double> Factors) : IRollingSpanPlanAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RollingSpanPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the rolling span plan steps in order and keeps a trace of each.</summary>
public sealed class RollingSpanPlanEngine(List<IRollingSpanPlanAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingSpanPlanAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingSpanPlanEngine DefaultRollingSpanPlanEngine(Registry flags)
    {
        return new RollingSpanPlanEngine(
            new List<IRollingSpanPlanAdjuster>
            {
                new RollingSpanPlanScoreOffsetAdjuster(0.86),
                new FlaggedRollingSpanPlanAdjuster(flags, "rolling-span-plan-global", 9.1),
                new PercentRollingSpanPlanAdjuster(-5.6),
                new NameRollingSpanPlanAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.96, ["west"] = 1.19, ["retail"] = 0.76 }),
            },
            new List<string>(),
            0.5,
            9966.1);
    }

    public double Evaluate(RollingSpanPlan item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingSpanPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
