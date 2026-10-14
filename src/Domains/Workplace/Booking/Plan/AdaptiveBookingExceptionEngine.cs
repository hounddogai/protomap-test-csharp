namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive booking exception calculation.</summary>
public interface IAdaptiveBookingExceptionStep
{
    string Describe();
    double Adjust(AdaptiveBookingException item, double valueValue);
}

public sealed record RoundingAdaptiveBookingExceptionAdjuster(double Step) : IAdaptiveBookingExceptionStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveBookingException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelAdaptiveBookingExceptionAdjuster(Dictionary<string, double> Factors) : IAdaptiveBookingExceptionStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AdaptiveBookingException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAdaptiveBookingExceptionAdjuster(double Percent) : IAdaptiveBookingExceptionStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveBookingException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive booking exception steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveBookingExceptionEngine(List<IAdaptiveBookingExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveBookingExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveBookingExceptionEngine DefaultAdaptiveBookingExceptionEngine()
    {
        return new AdaptiveBookingExceptionEngine(
            new List<IAdaptiveBookingExceptionStep>
            {
                new RoundingAdaptiveBookingExceptionAdjuster(0.5),
                new LabelAdaptiveBookingExceptionAdjuster(new Dictionary<string, double> { ["south"] = 0.93 }),
                new PercentAdaptiveBookingExceptionAdjuster(10.9),
            },
            new List<string>(),
            1.0,
            79646.0);
    }

    public double Evaluate(AdaptiveBookingException item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveBookingException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
