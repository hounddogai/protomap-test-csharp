namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted policy ack draft calculation.</summary>
public interface IWeightedPolicyAckDraftStep
{
    string Describe();
    double Adjust(WeightedPolicyAckDraft item, double valueValue);
}

public sealed record RoundingWeightedPolicyAckDraftAdjuster(double Step) : IWeightedPolicyAckDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedPolicyAckDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentWeightedPolicyAckDraftAdjuster(double Percent) : IWeightedPolicyAckDraftStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedPolicyAckDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryWeightedPolicyAckDraftAdjuster(Dictionary<string, double> Factors) : IWeightedPolicyAckDraftStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(WeightedPolicyAckDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapWeightedPolicyAckDraftAdjuster(double Cap) : IWeightedPolicyAckDraftStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedPolicyAckDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredWeightedPolicyAckDraftAdjuster(List<double> Bounds, List<double> Rates) : IWeightedPolicyAckDraftStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedPolicyAckDraft item, double valueValue)
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

/// <summary>Runs the weighted policy ack draft steps in order and keeps a trace of each.</summary>
public sealed class WeightedPolicyAckDraftEngine(List<IWeightedPolicyAckDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedPolicyAckDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedPolicyAckDraftEngine DefaultWeightedPolicyAckDraftEngine()
    {
        return new WeightedPolicyAckDraftEngine(
            new List<IWeightedPolicyAckDraftStep>
            {
                new RoundingWeightedPolicyAckDraftAdjuster(0.1),
                new PercentWeightedPolicyAckDraftAdjuster(-23.5),
                new CategoryWeightedPolicyAckDraftAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.01, ["south"] = 0.93, ["west"] = 0.99 }),
                new CapWeightedPolicyAckDraftAdjuster(19143.0),
                new TieredWeightedPolicyAckDraftAdjuster(
                    new List<double> { 3811.0, 3980.0 },
                    new List<double> { 1.09, 1.05 }),
            },
            new List<string>(),
            2.6,
            62386.1);
    }

    public double Compute(WeightedPolicyAckDraft item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedPolicyAckDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
