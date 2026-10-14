namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback nomination exception calculation.</summary>
public interface IFallbackNominationExceptionStep
{
    string Describe();
    double Adjust(FallbackNominationException item, double valueValue);
}

public sealed record PercentFallbackNominationExceptionAdjuster(double Percent) : IFallbackNominationExceptionStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackNominationException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFallbackNominationExceptionAdjuster(double Cap) : IFallbackNominationExceptionStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackNominationException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumFallbackNominationExceptionAdjuster(double Minimum) : IFallbackNominationExceptionStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackNominationException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingFallbackNominationExceptionAdjuster(double Step) : IFallbackNominationExceptionStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackNominationException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback nomination exception steps in order and keeps a trace of each.</summary>
public sealed class FallbackNominationExceptionEngine(List<IFallbackNominationExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackNominationExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackNominationExceptionEngine DefaultFallbackNominationExceptionEngine()
    {
        return new FallbackNominationExceptionEngine(
            new List<IFallbackNominationExceptionStep>
            {
                new PercentFallbackNominationExceptionAdjuster(16.8),
                new CapFallbackNominationExceptionAdjuster(43371.0),
                new MinimumFallbackNominationExceptionAdjuster(6.4),
                new RoundingFallbackNominationExceptionAdjuster(0.5),
            },
            new List<string>(),
            2.2,
            27732.1);
    }

    public double Price(FallbackNominationException item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackNominationException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
