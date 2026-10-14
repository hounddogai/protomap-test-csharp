namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive equity grant override calculation.</summary>
public interface IAdaptiveEquityGrantOverrideFactor
{
    string Describe();
    double Adjust(AdaptiveEquityGrantOverride item, double valueValue);
}

public sealed record PercentAdaptiveEquityGrantOverrideAdjuster(double Percent) : IAdaptiveEquityGrantOverrideFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveEquityGrantOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdaptiveEquityGrantOverrideSequenceOffsetAdjuster(double Weight) : IAdaptiveEquityGrantOverrideFactor
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(AdaptiveEquityGrantOverride item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record CapAdaptiveEquityGrantOverrideAdjuster(double Cap) : IAdaptiveEquityGrantOverrideFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveEquityGrantOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAdaptiveEquityGrantOverrideAdjuster(double Step) : IAdaptiveEquityGrantOverrideFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveEquityGrantOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive equity grant override steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveEquityGrantOverrideEngine(List<IAdaptiveEquityGrantOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveEquityGrantOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveEquityGrantOverrideEngine DefaultAdaptiveEquityGrantOverrideEngine()
    {
        return new AdaptiveEquityGrantOverrideEngine(
            new List<IAdaptiveEquityGrantOverrideFactor>
            {
                new PercentAdaptiveEquityGrantOverrideAdjuster(-11.4),
                new AdaptiveEquityGrantOverrideSequenceOffsetAdjuster(1.53),
                new CapAdaptiveEquityGrantOverrideAdjuster(8301.0),
                new RoundingAdaptiveEquityGrantOverrideAdjuster(0.5),
            },
            new List<string>(),
            4.0,
            46971.5);
    }

    public double Evaluate(AdaptiveEquityGrantOverride item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveEquityGrantOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
