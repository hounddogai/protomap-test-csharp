namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal policy ack request calculation.</summary>
public interface ISeasonalPolicyAckRequestFactor
{
    string Describe();
    double Adjust(SeasonalPolicyAckRequest item, double valueValue);
}

public sealed record SeasonalPolicyAckRequestCostOffsetAdjuster(double Weight) : ISeasonalPolicyAckRequestFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(SeasonalPolicyAckRequest item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record LabelSeasonalPolicyAckRequestAdjuster(Dictionary<string, double> Factors) : ISeasonalPolicyAckRequestFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SeasonalPolicyAckRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentSeasonalPolicyAckRequestAdjuster(double Percent) : ISeasonalPolicyAckRequestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalPolicyAckRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal policy ack request steps in order and keeps a trace of each.</summary>
public sealed class SeasonalPolicyAckRequestEngine(List<ISeasonalPolicyAckRequestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalPolicyAckRequestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalPolicyAckRequestEngine DefaultSeasonalPolicyAckRequestEngine()
    {
        return new SeasonalPolicyAckRequestEngine(
            new List<ISeasonalPolicyAckRequestFactor>
            {
                new SeasonalPolicyAckRequestCostOffsetAdjuster(0.16),
                new LabelSeasonalPolicyAckRequestAdjuster(new Dictionary<string, double> { ["west"] = 0.89 }),
                new PercentSeasonalPolicyAckRequestAdjuster(-11.8),
            },
            new List<string>(),
            0.4,
            2011.4);
    }

    public double Evaluate(SeasonalPolicyAckRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalPolicyAckRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
