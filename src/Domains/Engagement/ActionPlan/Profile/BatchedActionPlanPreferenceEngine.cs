namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched action plan preference calculation.</summary>
public interface IBatchedActionPlanPreferenceStep
{
    string Describe();
    double Adjust(BatchedActionPlanPreference item, double valueValue);
}

public sealed record BatchedActionPlanPreferenceTenureMonthsOffsetAdjuster(double Weight) : IBatchedActionPlanPreferenceStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(BatchedActionPlanPreference item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record TieredBatchedActionPlanPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IBatchedActionPlanPreferenceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedActionPlanPreference item, double valueValue)
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

public sealed record TeamBatchedActionPlanPreferenceAdjuster(Dictionary<string, double> Factors) : IBatchedActionPlanPreferenceStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(BatchedActionPlanPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBatchedActionPlanPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IBatchedActionPlanPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedActionPlanPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBatchedActionPlanPreferenceAdjuster(double Minimum) : IBatchedActionPlanPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedActionPlanPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the batched action plan preference steps in order and keeps a trace of each.</summary>
public sealed class BatchedActionPlanPreferenceEngine(List<IBatchedActionPlanPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedActionPlanPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedActionPlanPreferenceEngine DefaultBatchedActionPlanPreferenceEngine(Registry flags)
    {
        return new BatchedActionPlanPreferenceEngine(
            new List<IBatchedActionPlanPreferenceStep>
            {
                new BatchedActionPlanPreferenceTenureMonthsOffsetAdjuster(1.87),
                new TieredBatchedActionPlanPreferenceAdjuster(
                    new List<double> { 3327.0, 5325.0, 5457.0 },
                    new List<double> { 0.96, 1.15, 0.93 }),
                new TeamBatchedActionPlanPreferenceAdjuster(new Dictionary<string, double> { ["retail"] = 1.12 }),
                new FlaggedBatchedActionPlanPreferenceAdjuster(flags, "batched-action-plan-preference-default", -1.9),
                new MinimumBatchedActionPlanPreferenceAdjuster(4.9),
            },
            new List<string>(),
            3.3,
            11895.4);
    }

    public double Run(BatchedActionPlanPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedActionPlanPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
