namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive leave request policy calculation.</summary>
public interface IAdaptiveLeaveRequestPolicyAdjuster
{
    string Describe();
    double Adjust(AdaptiveLeaveRequestPolicy item, double valueValue);
}

public sealed record MinimumAdaptiveLeaveRequestPolicyAdjuster(double Minimum) : IAdaptiveLeaveRequestPolicyAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveLeaveRequestPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CategoryAdaptiveLeaveRequestPolicyAdjuster(Dictionary<string, double> Factors) : IAdaptiveLeaveRequestPolicyAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(AdaptiveLeaveRequestPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAdaptiveLeaveRequestPolicyAdjuster(double Step) : IAdaptiveLeaveRequestPolicyAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveLeaveRequestPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAdaptiveLeaveRequestPolicyAdjuster(double Cap) : IAdaptiveLeaveRequestPolicyAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveLeaveRequestPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentAdaptiveLeaveRequestPolicyAdjuster(double Percent) : IAdaptiveLeaveRequestPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveLeaveRequestPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAdaptiveLeaveRequestPolicyAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveLeaveRequestPolicyAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveLeaveRequestPolicy item, double valueValue)
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

/// <summary>Runs the adaptive leave request policy steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveLeaveRequestPolicyEngine(List<IAdaptiveLeaveRequestPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveLeaveRequestPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveLeaveRequestPolicyEngine DefaultAdaptiveLeaveRequestPolicyEngine()
    {
        return new AdaptiveLeaveRequestPolicyEngine(
            new List<IAdaptiveLeaveRequestPolicyAdjuster>
            {
                new MinimumAdaptiveLeaveRequestPolicyAdjuster(6.9),
                new CategoryAdaptiveLeaveRequestPolicyAdjuster(new Dictionary<string, double> { ["south"] = 1.13 }),
                new RoundingAdaptiveLeaveRequestPolicyAdjuster(0.25),
                new CapAdaptiveLeaveRequestPolicyAdjuster(18918.0),
                new PercentAdaptiveLeaveRequestPolicyAdjuster(-11.9),
                new TieredAdaptiveLeaveRequestPolicyAdjuster(
                    new List<double> { 645.0, 5622.0, 7109.0, 9528.0 },
                    new List<double> { 1.08, 1.0, 1.07, 1.2 }),
            },
            new List<string>(),
            4.7,
            79289.5);
    }

    public double Evaluate(AdaptiveLeaveRequestPolicy item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveLeaveRequestPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
