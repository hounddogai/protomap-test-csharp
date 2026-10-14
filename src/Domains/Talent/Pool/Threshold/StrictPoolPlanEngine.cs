namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the strict pool plan calculation.</summary>
public interface IStrictPoolPlanStep
{
    string Describe();
    double Adjust(StrictPoolPlan item, double valueValue);
}

public sealed record FlaggedStrictPoolPlanAdjuster(Registry Flags, string Flag, double Percent) : IStrictPoolPlanStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictPoolPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictPoolPlanAdjuster(List<double> Bounds, List<double> Rates) : IStrictPoolPlanStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictPoolPlan item, double valueValue)
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

public sealed record CapStrictPoolPlanAdjuster(double Cap) : IStrictPoolPlanStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictPoolPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelStrictPoolPlanAdjuster(Dictionary<string, double> Factors) : IStrictPoolPlanStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(StrictPoolPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the strict pool plan steps in order and keeps a trace of each.</summary>
public sealed class StrictPoolPlanEngine(List<IStrictPoolPlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPoolPlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPoolPlanEngine DefaultStrictPoolPlanEngine(Registry flags)
    {
        return new StrictPoolPlanEngine(
            new List<IStrictPoolPlanStep>
            {
                new FlaggedStrictPoolPlanAdjuster(flags, "strict-pool-plan-global", -8.7),
                new TieredStrictPoolPlanAdjuster(
                    new List<double> { 1718.0, 5787.0 },
                    new List<double> { 0.93, 0.84 }),
                new CapStrictPoolPlanAdjuster(22444.0),
                new LabelStrictPoolPlanAdjuster(new Dictionary<string, double> { ["east"] = 0.93, ["online"] = 1.0 }),
            },
            new List<string>(),
            4.5,
            53727.8);
    }

    public double Price(StrictPoolPlan item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StrictPoolPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
