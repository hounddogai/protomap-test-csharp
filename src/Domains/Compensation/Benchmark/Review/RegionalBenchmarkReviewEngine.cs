namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional benchmark review calculation.</summary>
public interface IRegionalBenchmarkReviewAdjuster
{
    string Describe();
    double Adjust(RegionalBenchmarkReview item, double valueValue);
}

public sealed record CapRegionalBenchmarkReviewAdjuster(double Cap) : IRegionalBenchmarkReviewAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalBenchmarkReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRegionalBenchmarkReviewAdjuster(double Percent) : IRegionalBenchmarkReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalBenchmarkReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRegionalBenchmarkReviewAdjuster(double Minimum) : IRegionalBenchmarkReviewAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalBenchmarkReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRegionalBenchmarkReviewAdjuster(Registry Flags, string Flag, double Percent) : IRegionalBenchmarkReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalBenchmarkReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRegionalBenchmarkReviewAdjuster(double Step) : IRegionalBenchmarkReviewAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalBenchmarkReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the regional benchmark review steps in order and keeps a trace of each.</summary>
public sealed class RegionalBenchmarkReviewEngine(List<IRegionalBenchmarkReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalBenchmarkReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalBenchmarkReviewEngine DefaultRegionalBenchmarkReviewEngine(Registry flags)
    {
        return new RegionalBenchmarkReviewEngine(
            new List<IRegionalBenchmarkReviewAdjuster>
            {
                new CapRegionalBenchmarkReviewAdjuster(2939.0),
                new PercentRegionalBenchmarkReviewAdjuster(-12.8),
                new MinimumRegionalBenchmarkReviewAdjuster(42.6),
                new FlaggedRegionalBenchmarkReviewAdjuster(flags, "regional-benchmark-review-adaptive", 8.3),
                new RoundingRegionalBenchmarkReviewAdjuster(0.5),
            },
            new List<string>(),
            3.5,
            16469.4);
    }

    public double Run(RegionalBenchmarkReview item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalBenchmarkReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
