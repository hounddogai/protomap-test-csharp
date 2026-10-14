namespace Acme.Workforce.Engagement.Sentiment.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling sentiment plan calculation.</summary>
public interface IRollingSentimentPlanModifier
{
    string Describe();
    double Adjust(RollingSentimentPlan item, double valueValue);
}

public sealed record RoundingRollingSentimentPlanAdjuster(double Step) : IRollingSentimentPlanModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingSentimentPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRollingSentimentPlanAdjuster(List<double> Bounds, List<double> Rates) : IRollingSentimentPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingSentimentPlan item, double valueValue)
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

public sealed record CapRollingSentimentPlanAdjuster(double Cap) : IRollingSentimentPlanModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingSentimentPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRollingSentimentPlanAdjuster(double Percent) : IRollingSentimentPlanModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingSentimentPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRollingSentimentPlanAdjuster(double Minimum) : IRollingSentimentPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingSentimentPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRollingSentimentPlanAdjuster(Registry Flags, string Flag, double Percent) : IRollingSentimentPlanModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingSentimentPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling sentiment plan steps in order and keeps a trace of each.</summary>
public sealed class RollingSentimentPlanEngine(List<IRollingSentimentPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingSentimentPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingSentimentPlanEngine DefaultRollingSentimentPlanEngine(Registry flags)
    {
        return new RollingSentimentPlanEngine(
            new List<IRollingSentimentPlanModifier>
            {
                new RoundingRollingSentimentPlanAdjuster(0.05),
                new TieredRollingSentimentPlanAdjuster(
                    new List<double> { 18.0, 599.0, 2548.0, 8101.0 },
                    new List<double> { 1.07, 1.09, 1.18, 1.05 }),
                new CapRollingSentimentPlanAdjuster(24741.0),
                new PercentRollingSentimentPlanAdjuster(-2.0),
                new MinimumRollingSentimentPlanAdjuster(1.7),
                new FlaggedRollingSentimentPlanAdjuster(flags, "rolling-sentiment-plan-global", -6.4),
            },
            new List<string>(),
            0.7,
            90235.4);
    }

    public double Run(RollingSentimentPlan item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RollingSentimentPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
