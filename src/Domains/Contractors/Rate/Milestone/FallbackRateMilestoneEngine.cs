namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback rate milestone calculation.</summary>
public interface IFallbackRateMilestoneAdjuster
{
    string Describe();
    double Adjust(FallbackRateMilestone item, double valueValue);
}

public sealed record CapFallbackRateMilestoneAdjuster(double Cap) : IFallbackRateMilestoneAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackRateMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentFallbackRateMilestoneAdjuster(double Percent) : IFallbackRateMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackRateMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackRateMilestoneMarginOffsetAdjuster(double Weight) : IFallbackRateMilestoneAdjuster
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(FallbackRateMilestone item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record MinimumFallbackRateMilestoneAdjuster(double Minimum) : IFallbackRateMilestoneAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackRateMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the fallback rate milestone steps in order and keeps a trace of each.</summary>
public sealed class FallbackRateMilestoneEngine(List<IFallbackRateMilestoneAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackRateMilestoneAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackRateMilestoneEngine DefaultFallbackRateMilestoneEngine()
    {
        return new FallbackRateMilestoneEngine(
            new List<IFallbackRateMilestoneAdjuster>
            {
                new CapFallbackRateMilestoneAdjuster(26909.0),
                new PercentFallbackRateMilestoneAdjuster(-20.5),
                new FallbackRateMilestoneMarginOffsetAdjuster(1.31),
                new MinimumFallbackRateMilestoneAdjuster(13.9),
            },
            new List<string>(),
            0.5,
            50608.0);
    }

    public double Price(FallbackRateMilestone item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackRateMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
