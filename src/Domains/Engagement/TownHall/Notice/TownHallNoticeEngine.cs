namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall notice calculation.</summary>
public interface ITownHallNoticeStep
{
    string Describe();
    double Adjust(TownHallNotice item, double valueValue);
}

public sealed record PercentTownHallNoticeAdjuster(double Percent) : ITownHallNoticeStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedTownHallNoticeAdjuster(Registry Flags, string Flag, double Percent) : ITownHallNoticeStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTownHallNoticeAdjuster(double Minimum) : ITownHallNoticeStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredTownHallNoticeAdjuster(List<double> Bounds, List<double> Rates) : ITownHallNoticeStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallNotice item, double valueValue)
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

/// <summary>Runs the town hall notice steps in order and keeps a trace of each.</summary>
public sealed class TownHallNoticeEngine(List<ITownHallNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallNoticeEngine DefaultTownHallNoticeEngine(Registry flags)
    {
        return new TownHallNoticeEngine(
            new List<ITownHallNoticeStep>
            {
                new PercentTownHallNoticeAdjuster(-23.1),
                new FlaggedTownHallNoticeAdjuster(flags, "town-hall-notice-pending", 9.4),
                new MinimumTownHallNoticeAdjuster(16.8),
                new TieredTownHallNoticeAdjuster(
                    new List<double> { 2133.0, 3265.0, 9552.0 },
                    new List<double> { 0.83, 1.04, 1.11 }),
            },
            new List<string>(),
            1.5,
            97198.5);
    }

    public double Run(TownHallNotice item)
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

    public Dictionary<string, double> EvaluateAll(List<TownHallNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
