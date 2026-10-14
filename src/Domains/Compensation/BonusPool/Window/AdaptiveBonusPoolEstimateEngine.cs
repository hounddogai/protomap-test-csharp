namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive bonus pool estimate calculation.</summary>
public interface IAdaptiveBonusPoolEstimateModifier
{
    string Describe();
    double Adjust(AdaptiveBonusPoolEstimate item, double valueValue);
}

public sealed record PercentAdaptiveBonusPoolEstimateAdjuster(double Percent) : IAdaptiveBonusPoolEstimateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveBonusPoolEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdaptiveBonusPoolEstimateAdjuster(double Cap) : IAdaptiveBonusPoolEstimateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveBonusPoolEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameAdaptiveBonusPoolEstimateAdjuster(Dictionary<string, double> Factors) : IAdaptiveBonusPoolEstimateModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AdaptiveBonusPoolEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAdaptiveBonusPoolEstimateAdjuster(double Step) : IAdaptiveBonusPoolEstimateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveBonusPoolEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive bonus pool estimate steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveBonusPoolEstimateEngine(List<IAdaptiveBonusPoolEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveBonusPoolEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveBonusPoolEstimateEngine DefaultAdaptiveBonusPoolEstimateEngine()
    {
        return new AdaptiveBonusPoolEstimateEngine(
            new List<IAdaptiveBonusPoolEstimateModifier>
            {
                new PercentAdaptiveBonusPoolEstimateAdjuster(15.0),
                new CapAdaptiveBonusPoolEstimateAdjuster(29599.0),
                new NameAdaptiveBonusPoolEstimateAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.99, ["east"] = 1.28 }),
                new RoundingAdaptiveBonusPoolEstimateAdjuster(0.5),
            },
            new List<string>(),
            3.9,
            94038.8);
    }

    public double Evaluate(AdaptiveBonusPoolEstimate item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveBonusPoolEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
