namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive town hall rule calculation.</summary>
public interface IAdaptiveTownHallRuleAdjuster
{
    string Describe();
    double Adjust(AdaptiveTownHallRule item, double valueValue);
}

public sealed record AdaptiveTownHallRuleRevisionOffsetAdjuster(double Weight) : IAdaptiveTownHallRuleAdjuster
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(AdaptiveTownHallRule item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record MinimumAdaptiveTownHallRuleAdjuster(double Minimum) : IAdaptiveTownHallRuleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveTownHallRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdaptiveTownHallRuleAdjuster(double Cap) : IAdaptiveTownHallRuleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveTownHallRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DepartmentAdaptiveTownHallRuleAdjuster(Dictionary<string, double> Factors) : IAdaptiveTownHallRuleAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(AdaptiveTownHallRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive town hall rule steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveTownHallRuleEngine(List<IAdaptiveTownHallRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveTownHallRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveTownHallRuleEngine DefaultAdaptiveTownHallRuleEngine()
    {
        return new AdaptiveTownHallRuleEngine(
            new List<IAdaptiveTownHallRuleAdjuster>
            {
                new AdaptiveTownHallRuleRevisionOffsetAdjuster(1.28),
                new MinimumAdaptiveTownHallRuleAdjuster(35.8),
                new CapAdaptiveTownHallRuleAdjuster(11319.0),
                new DepartmentAdaptiveTownHallRuleAdjuster(new Dictionary<string, double> { ["retail"] = 1.01 }),
            },
            new List<string>(),
            3.2,
            2949.0);
    }

    public double Price(AdaptiveTownHallRule item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveTownHallRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
