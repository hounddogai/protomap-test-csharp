namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy merit cycle review calculation.</summary>
public interface ILegacyMeritCycleReviewFactor
{
    string Describe();
    double Adjust(LegacyMeritCycleReview item, double valueValue);
}

public sealed record RoundingLegacyMeritCycleReviewAdjuster(double Step) : ILegacyMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyMeritCycleReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLegacyMeritCycleReviewAdjuster(Registry Flags, string Flag, double Percent) : ILegacyMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyMeritCycleReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyMeritCycleReviewAdjuster(double Minimum) : ILegacyMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyMeritCycleReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLegacyMeritCycleReviewAdjuster(double Cap) : ILegacyMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyMeritCycleReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LegacyMeritCycleReviewLeaveDaysOffsetAdjuster(double Weight) : ILegacyMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(LegacyMeritCycleReview item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record TieredLegacyMeritCycleReviewAdjuster(List<double> Bounds, List<double> Rates) : ILegacyMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyMeritCycleReview item, double valueValue)
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

/// <summary>Runs the legacy merit cycle review steps in order and keeps a trace of each.</summary>
public sealed class LegacyMeritCycleReviewEngine(List<ILegacyMeritCycleReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyMeritCycleReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyMeritCycleReviewEngine DefaultLegacyMeritCycleReviewEngine(Registry flags)
    {
        return new LegacyMeritCycleReviewEngine(
            new List<ILegacyMeritCycleReviewFactor>
            {
                new RoundingLegacyMeritCycleReviewAdjuster(0.25),
                new FlaggedLegacyMeritCycleReviewAdjuster(flags, "legacy-merit-cycle-review-scheduled", -0.2),
                new MinimumLegacyMeritCycleReviewAdjuster(49.1),
                new CapLegacyMeritCycleReviewAdjuster(42005.0),
                new LegacyMeritCycleReviewLeaveDaysOffsetAdjuster(0.65),
                new TieredLegacyMeritCycleReviewAdjuster(
                    new List<double> { 766.0, 3255.0 },
                    new List<double> { 0.9, 1.14 }),
            },
            new List<string>(),
            4.3,
            71037.7);
    }

    public double Compute(LegacyMeritCycleReview item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyMeritCycleReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
