namespace Acme.Workforce.Compensation.EquityGrant.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive equity grant target calculation.</summary>
public interface IAdaptiveEquityGrantTargetFactor
{
    string Describe();
    double Adjust(AdaptiveEquityGrantTarget item, double valueValue);
}

public sealed record CountryAdaptiveEquityGrantTargetAdjuster(Dictionary<string, double> Factors) : IAdaptiveEquityGrantTargetFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(AdaptiveEquityGrantTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAdaptiveEquityGrantTargetAdjuster(double Percent) : IAdaptiveEquityGrantTargetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveEquityGrantTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdaptiveEquityGrantTargetAdjuster(double Cap) : IAdaptiveEquityGrantTargetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveEquityGrantTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAdaptiveEquityGrantTargetAdjuster(double Minimum) : IAdaptiveEquityGrantTargetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveEquityGrantTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AdaptiveEquityGrantTargetPriorityOffsetAdjuster(double Weight) : IAdaptiveEquityGrantTargetFactor
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(AdaptiveEquityGrantTarget item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

/// <summary>Runs the adaptive equity grant target steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveEquityGrantTargetEngine(List<IAdaptiveEquityGrantTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveEquityGrantTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveEquityGrantTargetEngine DefaultAdaptiveEquityGrantTargetEngine()
    {
        return new AdaptiveEquityGrantTargetEngine(
            new List<IAdaptiveEquityGrantTargetFactor>
            {
                new CountryAdaptiveEquityGrantTargetAdjuster(new Dictionary<string, double> { ["south"] = 1.31 }),
                new PercentAdaptiveEquityGrantTargetAdjuster(-19.0),
                new CapAdaptiveEquityGrantTargetAdjuster(614.0),
                new MinimumAdaptiveEquityGrantTargetAdjuster(34.3),
                new AdaptiveEquityGrantTargetPriorityOffsetAdjuster(1.46),
            },
            new List<string>(),
            4.0,
            69271.3);
    }

    public double Compute(AdaptiveEquityGrantTarget item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveEquityGrantTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
