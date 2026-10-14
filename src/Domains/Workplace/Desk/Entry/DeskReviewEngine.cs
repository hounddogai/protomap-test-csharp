namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk review calculation.</summary>
public interface IDeskReviewAdjuster
{
    string Describe();
    double Adjust(DeskReview item, double valueValue);
}

public sealed record CapDeskReviewAdjuster(double Cap) : IDeskReviewAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeskReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameDeskReviewAdjuster(Dictionary<string, double> Factors) : IDeskReviewAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(DeskReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentDeskReviewAdjuster(double Percent) : IDeskReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDeskReviewAdjuster(double Step) : IDeskReviewAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeskReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDeskReviewAdjuster(Registry Flags, string Flag, double Percent) : IDeskReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDeskReviewAdjuster(List<double> Bounds, List<double> Rates) : IDeskReviewAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeskReview item, double valueValue)
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

/// <summary>Runs the desk review steps in order and keeps a trace of each.</summary>
public sealed class DeskReviewEngine(List<IDeskReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskReviewEngine DefaultDeskReviewEngine(Registry flags)
    {
        return new DeskReviewEngine(
            new List<IDeskReviewAdjuster>
            {
                new CapDeskReviewAdjuster(37378.0),
                new NameDeskReviewAdjuster(new Dictionary<string, double> { ["west"] = 0.93 }),
                new PercentDeskReviewAdjuster(-21.6),
                new RoundingDeskReviewAdjuster(0.05),
                new FlaggedDeskReviewAdjuster(flags, "desk-review-seasonal", -1.6),
                new TieredDeskReviewAdjuster(
                    new List<double> { 4002.0, 5787.0, 7618.0 },
                    new List<double> { 1.18, 0.86, 1.14 }),
            },
            new List<string>(),
            2.3,
            51464.5);
    }

    public double Price(DeskReview item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<DeskReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
