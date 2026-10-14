namespace Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default succession plan variance calculation.</summary>
public interface IDefaultSuccessionPlanVarianceAdjuster
{
    string Describe();
    double Adjust(DefaultSuccessionPlanVariance item, double valueValue);
}

public sealed record MinimumDefaultSuccessionPlanVarianceAdjuster(double Minimum) : IDefaultSuccessionPlanVarianceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultSuccessionPlanVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDefaultSuccessionPlanVarianceAdjuster(Registry Flags, string Flag, double Percent) : IDefaultSuccessionPlanVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultSuccessionPlanVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DefaultSuccessionPlanVarianceThresholdOffsetAdjuster(double Weight) : IDefaultSuccessionPlanVarianceAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(DefaultSuccessionPlanVariance item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record TitleDefaultSuccessionPlanVarianceAdjuster(Dictionary<string, double> Factors) : IDefaultSuccessionPlanVarianceAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DefaultSuccessionPlanVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDefaultSuccessionPlanVarianceAdjuster(double Step) : IDefaultSuccessionPlanVarianceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultSuccessionPlanVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the default succession plan variance steps in order and keeps a trace of each.</summary>
public sealed class DefaultSuccessionPlanVarianceEngine(List<IDefaultSuccessionPlanVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultSuccessionPlanVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultSuccessionPlanVarianceEngine DefaultDefaultSuccessionPlanVarianceEngine(Registry flags)
    {
        return new DefaultSuccessionPlanVarianceEngine(
            new List<IDefaultSuccessionPlanVarianceAdjuster>
            {
                new MinimumDefaultSuccessionPlanVarianceAdjuster(44.4),
                new FlaggedDefaultSuccessionPlanVarianceAdjuster(
                    flags,
                    "default-succession-plan-variance-global",
                    9.0),
                new DefaultSuccessionPlanVarianceThresholdOffsetAdjuster(1.1),
                new TitleDefaultSuccessionPlanVarianceAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.74, ["central"] = 1.09, ["north"] = 0.75 }),
                new RoundingDefaultSuccessionPlanVarianceAdjuster(0.1),
            },
            new List<string>(),
            1.6,
            33003.3);
    }

    public double Evaluate(DefaultSuccessionPlanVariance item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultSuccessionPlanVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
