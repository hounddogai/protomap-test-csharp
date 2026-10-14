namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback pay band mapping calculation.</summary>
public interface IFallbackPayBandMappingAdjuster
{
    string Describe();
    double Adjust(FallbackPayBandMapping item, double valueValue);
}

public sealed record RoundingFallbackPayBandMappingAdjuster(double Step) : IFallbackPayBandMappingAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackPayBandMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFallbackPayBandMappingAdjuster(double Percent) : IFallbackPayBandMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackPayBandMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFallbackPayBandMappingAdjuster(double Cap) : IFallbackPayBandMappingAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackPayBandMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the fallback pay band mapping steps in order and keeps a trace of each.</summary>
public sealed class FallbackPayBandMappingEngine(List<IFallbackPayBandMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPayBandMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPayBandMappingEngine DefaultFallbackPayBandMappingEngine()
    {
        return new FallbackPayBandMappingEngine(
            new List<IFallbackPayBandMappingAdjuster>
            {
                new RoundingFallbackPayBandMappingAdjuster(1.0),
                new PercentFallbackPayBandMappingAdjuster(-0.2),
                new CapFallbackPayBandMappingAdjuster(42870.0),
            },
            new List<string>(),
            3.0,
            66374.7);
    }

    public double Compute(FallbackPayBandMapping item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPayBandMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
