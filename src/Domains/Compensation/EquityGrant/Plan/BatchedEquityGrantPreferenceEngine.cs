namespace Acme.Workforce.Compensation.EquityGrant.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched equity grant preference calculation.</summary>
public interface IBatchedEquityGrantPreferenceStep
{
    string Describe();
    double Adjust(BatchedEquityGrantPreference item, double valueValue);
}

public sealed record BatchedEquityGrantPreferenceFloorValueOffsetAdjuster(double Weight) : IBatchedEquityGrantPreferenceStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(BatchedEquityGrantPreference item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record FlaggedBatchedEquityGrantPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IBatchedEquityGrantPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedEquityGrantPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamBatchedEquityGrantPreferenceAdjuster(Dictionary<string, double> Factors) : IBatchedEquityGrantPreferenceStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(BatchedEquityGrantPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the batched equity grant preference steps in order and keeps a trace of each.</summary>
public sealed class BatchedEquityGrantPreferenceEngine(List<IBatchedEquityGrantPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedEquityGrantPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedEquityGrantPreferenceEngine DefaultBatchedEquityGrantPreferenceEngine(Registry flags)
    {
        return new BatchedEquityGrantPreferenceEngine(
            new List<IBatchedEquityGrantPreferenceStep>
            {
                new BatchedEquityGrantPreferenceFloorValueOffsetAdjuster(0.7),
                new FlaggedBatchedEquityGrantPreferenceAdjuster(flags, "batched-equity-grant-preference-tiered", 9.2),
                new TeamBatchedEquityGrantPreferenceAdjuster(new Dictionary<string, double> { ["east"] = 0.81 }),
            },
            new List<string>(),
            1.2,
            83064.6);
    }

    public double Run(BatchedEquityGrantPreference item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedEquityGrantPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
