namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive career path notice calculation.</summary>
public interface IAdaptiveCareerPathNoticeAdjuster
{
    string Describe();
    double Adjust(AdaptiveCareerPathNotice item, double valueValue);
}

public sealed record PercentAdaptiveCareerPathNoticeAdjuster(double Percent) : IAdaptiveCareerPathNoticeAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveCareerPathNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAdaptiveCareerPathNoticeAdjuster(double Step) : IAdaptiveCareerPathNoticeAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveCareerPathNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAdaptiveCareerPathNoticeAdjuster(double Cap) : IAdaptiveCareerPathNoticeAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveCareerPathNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AdaptiveCareerPathNoticeDurationMinutesOffsetAdjuster(double Weight) : IAdaptiveCareerPathNoticeAdjuster
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(AdaptiveCareerPathNotice item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record MinimumAdaptiveCareerPathNoticeAdjuster(double Minimum) : IAdaptiveCareerPathNoticeAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveCareerPathNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelAdaptiveCareerPathNoticeAdjuster(Dictionary<string, double> Factors) : IAdaptiveCareerPathNoticeAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AdaptiveCareerPathNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive career path notice steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveCareerPathNoticeEngine(List<IAdaptiveCareerPathNoticeAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveCareerPathNoticeAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveCareerPathNoticeEngine DefaultAdaptiveCareerPathNoticeEngine()
    {
        return new AdaptiveCareerPathNoticeEngine(
            new List<IAdaptiveCareerPathNoticeAdjuster>
            {
                new PercentAdaptiveCareerPathNoticeAdjuster(-18.0),
                new RoundingAdaptiveCareerPathNoticeAdjuster(5.0),
                new CapAdaptiveCareerPathNoticeAdjuster(19293.0),
                new AdaptiveCareerPathNoticeDurationMinutesOffsetAdjuster(1.42),
                new MinimumAdaptiveCareerPathNoticeAdjuster(13.9),
                new LabelAdaptiveCareerPathNoticeAdjuster(new Dictionary<string, double> { ["retail"] = 1.2 }),
            },
            new List<string>(),
            0.6,
            79159.0);
    }

    public double Evaluate(AdaptiveCareerPathNotice item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveCareerPathNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
