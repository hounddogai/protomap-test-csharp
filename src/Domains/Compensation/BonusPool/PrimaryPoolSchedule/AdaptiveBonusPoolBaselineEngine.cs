namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive bonus pool baseline calculation.</summary>
public interface IAdaptiveBonusPoolBaselineModifier
{
    string Describe();
    double Adjust(AdaptiveBonusPoolBaseline item, double valueValue);
}

public sealed record PercentAdaptiveBonusPoolBaselineAdjuster(double Percent) : IAdaptiveBonusPoolBaselineModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveBonusPoolBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAdaptiveBonusPoolBaselineAdjuster(double Step) : IAdaptiveBonusPoolBaselineModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveBonusPoolBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAdaptiveBonusPoolBaselineAdjuster(double Minimum) : IAdaptiveBonusPoolBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveBonusPoolBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive bonus pool baseline steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveBonusPoolBaselineEngine(List<IAdaptiveBonusPoolBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveBonusPoolBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveBonusPoolBaselineEngine DefaultAdaptiveBonusPoolBaselineEngine()
    {
        return new AdaptiveBonusPoolBaselineEngine(
            new List<IAdaptiveBonusPoolBaselineModifier>
            {
                new PercentAdaptiveBonusPoolBaselineAdjuster(0.3),
                new RoundingAdaptiveBonusPoolBaselineAdjuster(5.0),
                new MinimumAdaptiveBonusPoolBaselineAdjuster(3.2),
            },
            new List<string>(),
            3.3,
            85821.7);
    }

    public double Price(AdaptiveBonusPoolBaseline item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveBonusPoolBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
