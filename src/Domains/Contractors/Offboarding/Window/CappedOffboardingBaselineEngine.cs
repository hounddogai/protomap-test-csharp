namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped offboarding baseline calculation.</summary>
public interface ICappedOffboardingBaselineModifier
{
    string Describe();
    double Adjust(CappedOffboardingBaseline item, double valueValue);
}

public sealed record CappedOffboardingBaselineRatingOffsetAdjuster(double Weight) : ICappedOffboardingBaselineModifier
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(CappedOffboardingBaseline item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record MinimumCappedOffboardingBaselineAdjuster(double Minimum) : ICappedOffboardingBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedOffboardingBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapCappedOffboardingBaselineAdjuster(double Cap) : ICappedOffboardingBaselineModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedOffboardingBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SegmentCappedOffboardingBaselineAdjuster(Dictionary<string, double> Factors) : ICappedOffboardingBaselineModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(CappedOffboardingBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the capped offboarding baseline steps in order and keeps a trace of each.</summary>
public sealed class CappedOffboardingBaselineEngine(List<ICappedOffboardingBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedOffboardingBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedOffboardingBaselineEngine DefaultCappedOffboardingBaselineEngine()
    {
        return new CappedOffboardingBaselineEngine(
            new List<ICappedOffboardingBaselineModifier>
            {
                new CappedOffboardingBaselineRatingOffsetAdjuster(1.81),
                new MinimumCappedOffboardingBaselineAdjuster(48.6),
                new CapCappedOffboardingBaselineAdjuster(35384.0),
                new SegmentCappedOffboardingBaselineAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.38, ["south"] = 1.17 }),
            },
            new List<string>(),
            4.3,
            89162.4);
    }

    public double Evaluate(CappedOffboardingBaseline item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedOffboardingBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
