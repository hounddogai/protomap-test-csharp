namespace Acme.Workforce.Contractors.Engagement.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted engagement exception calculation.</summary>
public interface IWeightedEngagementExceptionStep
{
    string Describe();
    double Adjust(WeightedEngagementException item, double valueValue);
}

public sealed record MinimumWeightedEngagementExceptionAdjuster(double Minimum) : IWeightedEngagementExceptionStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedEngagementException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameWeightedEngagementExceptionAdjuster(Dictionary<string, double> Factors) : IWeightedEngagementExceptionStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(WeightedEngagementException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedWeightedEngagementExceptionAdjuster(Registry Flags, string Flag, double Percent) : IWeightedEngagementExceptionStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedEngagementException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWeightedEngagementExceptionAdjuster(List<double> Bounds, List<double> Rates) : IWeightedEngagementExceptionStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedEngagementException item, double valueValue)
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

public sealed record RoundingWeightedEngagementExceptionAdjuster(double Step) : IWeightedEngagementExceptionStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedEngagementException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapWeightedEngagementExceptionAdjuster(double Cap) : IWeightedEngagementExceptionStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedEngagementException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the weighted engagement exception steps in order and keeps a trace of each.</summary>
public sealed class WeightedEngagementExceptionEngine(List<IWeightedEngagementExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedEngagementExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedEngagementExceptionEngine DefaultWeightedEngagementExceptionEngine(Registry flags)
    {
        return new WeightedEngagementExceptionEngine(
            new List<IWeightedEngagementExceptionStep>
            {
                new MinimumWeightedEngagementExceptionAdjuster(39.1),
                new NameWeightedEngagementExceptionAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.07, ["north"] = 0.73 }),
                new FlaggedWeightedEngagementExceptionAdjuster(flags, "weighted-engagement-exception-default", 9.3),
                new TieredWeightedEngagementExceptionAdjuster(
                    new List<double> { 1041.0, 3933.0, 4797.0, 5147.0 },
                    new List<double> { 0.81, 1.13, 1.06, 0.98 }),
                new RoundingWeightedEngagementExceptionAdjuster(5.0),
                new CapWeightedEngagementExceptionAdjuster(44704.0),
            },
            new List<string>(),
            4.3,
            48453.6);
    }

    public double Evaluate(WeightedEngagementException item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedEngagementException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
