namespace Acme.Workforce.Engagement.TownHall.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive town hall notice calculation.</summary>
public interface IAdaptiveTownHallNoticeFactor
{
    string Describe();
    double Adjust(AdaptiveTownHallNotice item, double valueValue);
}

public sealed record MinimumAdaptiveTownHallNoticeAdjuster(double Minimum) : IAdaptiveTownHallNoticeFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveTownHallNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAdaptiveTownHallNoticeAdjuster(double Step) : IAdaptiveTownHallNoticeFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveTownHallNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAdaptiveTownHallNoticeAdjuster(double Percent) : IAdaptiveTownHallNoticeFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveTownHallNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdaptiveTownHallNoticeAdjuster(double Cap) : IAdaptiveTownHallNoticeFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveTownHallNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AdaptiveTownHallNoticeBudgetOffsetAdjuster(double Weight) : IAdaptiveTownHallNoticeFactor
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(AdaptiveTownHallNotice item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

/// <summary>Runs the adaptive town hall notice steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveTownHallNoticeEngine(List<IAdaptiveTownHallNoticeFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveTownHallNoticeFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveTownHallNoticeEngine DefaultAdaptiveTownHallNoticeEngine()
    {
        return new AdaptiveTownHallNoticeEngine(
            new List<IAdaptiveTownHallNoticeFactor>
            {
                new MinimumAdaptiveTownHallNoticeAdjuster(13.8),
                new RoundingAdaptiveTownHallNoticeAdjuster(1.0),
                new PercentAdaptiveTownHallNoticeAdjuster(-18.3),
                new CapAdaptiveTownHallNoticeAdjuster(18000.0),
                new AdaptiveTownHallNoticeBudgetOffsetAdjuster(1.7),
            },
            new List<string>(),
            4.7,
            11505.4);
    }

    public double Compute(AdaptiveTownHallNotice item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveTownHallNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
