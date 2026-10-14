namespace Acme.Workforce.Orgdesign.Headcount.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive headcount exception calculation.</summary>
public interface IAdaptiveHeadcountExceptionModifier
{
    string Describe();
    double Adjust(AdaptiveHeadcountException item, double valueValue);
}

public sealed record PercentAdaptiveHeadcountExceptionAdjuster(double Percent) : IAdaptiveHeadcountExceptionModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveHeadcountException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdaptiveHeadcountExceptionAdjuster(double Cap) : IAdaptiveHeadcountExceptionModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveHeadcountException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAdaptiveHeadcountExceptionAdjuster(double Step) : IAdaptiveHeadcountExceptionModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveHeadcountException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record AdaptiveHeadcountExceptionRatingOffsetAdjuster(double Weight) : IAdaptiveHeadcountExceptionModifier
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(AdaptiveHeadcountException item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record MinimumAdaptiveHeadcountExceptionAdjuster(double Minimum) : IAdaptiveHeadcountExceptionModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveHeadcountException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive headcount exception steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveHeadcountExceptionEngine(List<IAdaptiveHeadcountExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveHeadcountExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveHeadcountExceptionEngine DefaultAdaptiveHeadcountExceptionEngine()
    {
        return new AdaptiveHeadcountExceptionEngine(
            new List<IAdaptiveHeadcountExceptionModifier>
            {
                new PercentAdaptiveHeadcountExceptionAdjuster(13.3),
                new CapAdaptiveHeadcountExceptionAdjuster(18510.0),
                new RoundingAdaptiveHeadcountExceptionAdjuster(5.0),
                new AdaptiveHeadcountExceptionRatingOffsetAdjuster(0.4),
                new MinimumAdaptiveHeadcountExceptionAdjuster(27.6),
            },
            new List<string>(),
            3.8,
            60309.3);
    }

    public double Run(AdaptiveHeadcountException item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveHeadcountException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
