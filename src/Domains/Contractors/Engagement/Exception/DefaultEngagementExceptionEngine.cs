namespace Acme.Workforce.Contractors.Engagement.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default engagement exception calculation.</summary>
public interface IDefaultEngagementExceptionModifier
{
    string Describe();
    double Adjust(DefaultEngagementException item, double valueValue);
}

public sealed record DefaultEngagementExceptionRateOffsetAdjuster(double Weight) : IDefaultEngagementExceptionModifier
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(DefaultEngagementException item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record RoundingDefaultEngagementExceptionAdjuster(double Step) : IDefaultEngagementExceptionModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultEngagementException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultEngagementExceptionAdjuster(double Percent) : IDefaultEngagementExceptionModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultEngagementException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultEngagementExceptionAdjuster(double Minimum) : IDefaultEngagementExceptionModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultEngagementException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapDefaultEngagementExceptionAdjuster(double Cap) : IDefaultEngagementExceptionModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultEngagementException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the default engagement exception steps in order and keeps a trace of each.</summary>
public sealed class DefaultEngagementExceptionEngine(List<IDefaultEngagementExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultEngagementExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultEngagementExceptionEngine DefaultDefaultEngagementExceptionEngine()
    {
        return new DefaultEngagementExceptionEngine(
            new List<IDefaultEngagementExceptionModifier>
            {
                new DefaultEngagementExceptionRateOffsetAdjuster(1.97),
                new RoundingDefaultEngagementExceptionAdjuster(0.5),
                new PercentDefaultEngagementExceptionAdjuster(1.3),
                new MinimumDefaultEngagementExceptionAdjuster(10.4),
                new CapDefaultEngagementExceptionAdjuster(6504.0),
            },
            new List<string>(),
            4.3,
            28903.5);
    }

    public double Price(DefaultEngagementException item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultEngagementException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
