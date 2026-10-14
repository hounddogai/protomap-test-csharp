namespace Acme.Workforce.Compensation.MeritCycle.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient merit cycle review calculation.</summary>
public interface ILenientMeritCycleReviewFactor
{
    string Describe();
    double Adjust(LenientMeritCycleReview item, double valueValue);
}

public sealed record LenientMeritCycleReviewMaximumOffsetAdjuster(double Weight) : ILenientMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(LenientMeritCycleReview item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record FlaggedLenientMeritCycleReviewAdjuster(Registry Flags, string Flag, double Percent) : ILenientMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientMeritCycleReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientMeritCycleReviewAdjuster(double Minimum) : ILenientMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientMeritCycleReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLenientMeritCycleReviewAdjuster(List<double> Bounds, List<double> Rates) : ILenientMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientMeritCycleReview item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

/// <summary>Runs the lenient merit cycle review steps in order and keeps a trace of each.</summary>
public sealed class LenientMeritCycleReviewEngine(List<ILenientMeritCycleReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientMeritCycleReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientMeritCycleReviewEngine DefaultLenientMeritCycleReviewEngine(Registry flags)
    {
        return new LenientMeritCycleReviewEngine(
            new List<ILenientMeritCycleReviewFactor>
            {
                new LenientMeritCycleReviewMaximumOffsetAdjuster(1.14),
                new FlaggedLenientMeritCycleReviewAdjuster(flags, "lenient-merit-cycle-review-rolling", 1.4),
                new MinimumLenientMeritCycleReviewAdjuster(9.0),
                new TieredLenientMeritCycleReviewAdjuster(
                    new List<double> { 418.0, 4515.0, 6846.0 },
                    new List<double> { 1.09, 0.91, 1.05 }),
            },
            new List<string>(),
            4.7,
            89214.5);
    }

    public double Price(LenientMeritCycleReview item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<LenientMeritCycleReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
