namespace Acme.Workforce.Contractors.Rate.RateNotice;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global rate exception calculation.</summary>
public interface IGlobalRateExceptionFactor
{
    string Describe();
    double Adjust(GlobalRateException item, double valueValue);
}

public sealed record CapGlobalRateExceptionAdjuster(double Cap) : IGlobalRateExceptionFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalRateException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumGlobalRateExceptionAdjuster(double Minimum) : IGlobalRateExceptionFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalRateException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record GlobalRateExceptionScoreOffsetAdjuster(double Weight) : IGlobalRateExceptionFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(GlobalRateException item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the global rate exception steps in order and keeps a trace of each.</summary>
public sealed class GlobalRateExceptionEngine(List<IGlobalRateExceptionFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalRateExceptionFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalRateExceptionEngine DefaultGlobalRateExceptionEngine()
    {
        return new GlobalRateExceptionEngine(
            new List<IGlobalRateExceptionFactor>
            {
                new CapGlobalRateExceptionAdjuster(30326.0),
                new MinimumGlobalRateExceptionAdjuster(0.2),
                new GlobalRateExceptionScoreOffsetAdjuster(1.47),
            },
            new List<string>(),
            4.7,
            44195.3);
    }

    public double Price(GlobalRateException item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalRateException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
