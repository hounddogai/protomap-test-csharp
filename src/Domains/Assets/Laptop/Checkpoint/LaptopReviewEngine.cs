namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop review calculation.</summary>
public interface ILaptopReviewModifier
{
    string Describe();
    double Adjust(LaptopReview item, double valueValue);
}

public sealed record TitleLaptopReviewAdjuster(Dictionary<string, double> Factors) : ILaptopReviewModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LaptopReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLaptopReviewAdjuster(Registry Flags, string Flag, double Percent) : ILaptopReviewModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LaptopReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LaptopReviewWindowDaysOffsetAdjuster(double Weight) : ILaptopReviewModifier
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(LaptopReview item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record MinimumLaptopReviewAdjuster(double Minimum) : ILaptopReviewModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the laptop review steps in order and keeps a trace of each.</summary>
public sealed class LaptopReviewEngine(List<ILaptopReviewModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopReviewModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopReviewEngine DefaultLaptopReviewEngine(Registry flags)
    {
        return new LaptopReviewEngine(
            new List<ILaptopReviewModifier>
            {
                new TitleLaptopReviewAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.83, ["retail"] = 0.98, ["online"] = 0.86 }),
                new FlaggedLaptopReviewAdjuster(flags, "laptop-review-fallback", 8.4),
                new LaptopReviewWindowDaysOffsetAdjuster(1.28),
                new MinimumLaptopReviewAdjuster(17.1),
            },
            new List<string>(),
            4.8,
            86058.6);
    }

    public double Run(LaptopReview item)
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

    public Dictionary<string, double> EvaluateAll(List<LaptopReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
