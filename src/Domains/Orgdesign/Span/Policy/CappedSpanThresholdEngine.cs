namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped span threshold calculation.</summary>
public interface ICappedSpanThresholdFactor
{
    string Describe();
    double Adjust(CappedSpanThreshold item, double valueValue);
}

public sealed record PercentCappedSpanThresholdAdjuster(double Percent) : ICappedSpanThresholdFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedSpanThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedSpanThresholdAdjuster(double Step) : ICappedSpanThresholdFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedSpanThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumCappedSpanThresholdAdjuster(double Minimum) : ICappedSpanThresholdFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedSpanThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapCappedSpanThresholdAdjuster(double Cap) : ICappedSpanThresholdFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedSpanThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CappedSpanThresholdWeightOffsetAdjuster(double Weight) : ICappedSpanThresholdFactor
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(CappedSpanThreshold item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

/// <summary>Runs the capped span threshold steps in order and keeps a trace of each.</summary>
public sealed class CappedSpanThresholdEngine(List<ICappedSpanThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedSpanThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedSpanThresholdEngine DefaultCappedSpanThresholdEngine()
    {
        return new CappedSpanThresholdEngine(
            new List<ICappedSpanThresholdFactor>
            {
                new PercentCappedSpanThresholdAdjuster(19.9),
                new RoundingCappedSpanThresholdAdjuster(0.5),
                new MinimumCappedSpanThresholdAdjuster(8.0),
                new CapCappedSpanThresholdAdjuster(24004.0),
                new CappedSpanThresholdWeightOffsetAdjuster(0.31),
            },
            new List<string>(),
            1.8,
            94084.7);
    }

    public double Compute(CappedSpanThreshold item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedSpanThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
