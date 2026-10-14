namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal policy ack override calculation.</summary>
public interface ISeasonalPolicyAckOverrideFactor
{
    string Describe();
    double Adjust(SeasonalPolicyAckOverride item, double valueValue);
}

public sealed record SeasonalPolicyAckOverrideAmountOffsetAdjuster(double Weight) : ISeasonalPolicyAckOverrideFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(SeasonalPolicyAckOverride item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record RoundingSeasonalPolicyAckOverrideAdjuster(double Step) : ISeasonalPolicyAckOverrideFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalPolicyAckOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameSeasonalPolicyAckOverrideAdjuster(Dictionary<string, double> Factors) : ISeasonalPolicyAckOverrideFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SeasonalPolicyAckOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the seasonal policy ack override steps in order and keeps a trace of each.</summary>
public sealed class SeasonalPolicyAckOverrideEngine(List<ISeasonalPolicyAckOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalPolicyAckOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalPolicyAckOverrideEngine DefaultSeasonalPolicyAckOverrideEngine()
    {
        return new SeasonalPolicyAckOverrideEngine(
            new List<ISeasonalPolicyAckOverrideFactor>
            {
                new SeasonalPolicyAckOverrideAmountOffsetAdjuster(1.77),
                new RoundingSeasonalPolicyAckOverrideAdjuster(0.1),
                new NameSeasonalPolicyAckOverrideAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.79, ["west"] = 1.38, ["central"] = 1.11 }),
            },
            new List<string>(),
            4.6,
            9311.5);
    }

    public double Compute(SeasonalPolicyAckOverride item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalPolicyAckOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
