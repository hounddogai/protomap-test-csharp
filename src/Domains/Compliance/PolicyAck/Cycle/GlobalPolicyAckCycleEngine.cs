namespace Acme.Workforce.Compliance.PolicyAck.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global policy ack cycle calculation.</summary>
public interface IGlobalPolicyAckCycleAdjuster
{
    string Describe();
    double Adjust(GlobalPolicyAckCycle item, double valueValue);
}

public sealed record CapGlobalPolicyAckCycleAdjuster(double Cap) : IGlobalPolicyAckCycleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalPolicyAckCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleGlobalPolicyAckCycleAdjuster(Dictionary<string, double> Factors) : IGlobalPolicyAckCycleAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(GlobalPolicyAckCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedGlobalPolicyAckCycleAdjuster(Registry Flags, string Flag, double Percent) : IGlobalPolicyAckCycleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalPolicyAckCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record GlobalPolicyAckCycleRetriesOffsetAdjuster(double Weight) : IGlobalPolicyAckCycleAdjuster
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(GlobalPolicyAckCycle item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record TieredGlobalPolicyAckCycleAdjuster(List<double> Bounds, List<double> Rates) : IGlobalPolicyAckCycleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalPolicyAckCycle item, double valueValue)
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

public sealed record RoundingGlobalPolicyAckCycleAdjuster(double Step) : IGlobalPolicyAckCycleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalPolicyAckCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global policy ack cycle steps in order and keeps a trace of each.</summary>
public sealed class GlobalPolicyAckCycleEngine(List<IGlobalPolicyAckCycleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPolicyAckCycleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPolicyAckCycleEngine DefaultGlobalPolicyAckCycleEngine(Registry flags)
    {
        return new GlobalPolicyAckCycleEngine(
            new List<IGlobalPolicyAckCycleAdjuster>
            {
                new CapGlobalPolicyAckCycleAdjuster(13897.0),
                new TitleGlobalPolicyAckCycleAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.92, ["online"] = 1.39, ["retail"] = 1.3 }),
                new FlaggedGlobalPolicyAckCycleAdjuster(flags, "global-policy-ack-cycle-seasonal", -8.8),
                new GlobalPolicyAckCycleRetriesOffsetAdjuster(1.33),
                new TieredGlobalPolicyAckCycleAdjuster(
                    new List<double> { 1281.0, 1665.0, 3727.0 },
                    new List<double> { 0.89, 0.99, 0.89 }),
                new RoundingGlobalPolicyAckCycleAdjuster(0.5),
            },
            new List<string>(),
            3.8,
            93857.5);
    }

    public double Evaluate(GlobalPolicyAckCycle item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalPolicyAckCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
