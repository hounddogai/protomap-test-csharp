namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive holiday variance calculation.</summary>
public interface IAdaptiveHolidayVarianceFactor
{
    string Describe();
    double Adjust(AdaptiveHolidayVariance item, double valueValue);
}

public sealed record AdaptiveHolidayVarianceAmountOffsetAdjuster(double Weight) : IAdaptiveHolidayVarianceFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(AdaptiveHolidayVariance item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record RoundingAdaptiveHolidayVarianceAdjuster(double Step) : IAdaptiveHolidayVarianceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveHolidayVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelAdaptiveHolidayVarianceAdjuster(Dictionary<string, double> Factors) : IAdaptiveHolidayVarianceFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AdaptiveHolidayVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAdaptiveHolidayVarianceAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveHolidayVarianceFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveHolidayVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptiveHolidayVarianceAdjuster(double Minimum) : IAdaptiveHolidayVarianceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveHolidayVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive holiday variance steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveHolidayVarianceEngine(List<IAdaptiveHolidayVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveHolidayVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveHolidayVarianceEngine DefaultAdaptiveHolidayVarianceEngine(Registry flags)
    {
        return new AdaptiveHolidayVarianceEngine(
            new List<IAdaptiveHolidayVarianceFactor>
            {
                new AdaptiveHolidayVarianceAmountOffsetAdjuster(1.27),
                new RoundingAdaptiveHolidayVarianceAdjuster(0.05),
                new LabelAdaptiveHolidayVarianceAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.35, ["west"] = 0.91 }),
                new FlaggedAdaptiveHolidayVarianceAdjuster(flags, "adaptive-holiday-variance-regional", 1.9),
                new MinimumAdaptiveHolidayVarianceAdjuster(39.0),
            },
            new List<string>(),
            3.0,
            27407.1);
    }

    public double Evaluate(AdaptiveHolidayVariance item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveHolidayVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
