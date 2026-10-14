namespace Acme.Workforce.Engagement.TownHall.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional town hall group calculation.</summary>
public interface IRegionalTownHallGroupAdjuster
{
    string Describe();
    double Adjust(RegionalTownHallGroup item, double valueValue);
}

public sealed record PercentRegionalTownHallGroupAdjuster(double Percent) : IRegionalTownHallGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalTownHallGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRegionalTownHallGroupAdjuster(double Step) : IRegionalTownHallGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalTownHallGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRegionalTownHallGroupAdjuster(double Cap) : IRegionalTownHallGroupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalTownHallGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the regional town hall group steps in order and keeps a trace of each.</summary>
public sealed class RegionalTownHallGroupEngine(List<IRegionalTownHallGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalTownHallGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalTownHallGroupEngine DefaultRegionalTownHallGroupEngine()
    {
        return new RegionalTownHallGroupEngine(
            new List<IRegionalTownHallGroupAdjuster>
            {
                new PercentRegionalTownHallGroupAdjuster(8.4),
                new RoundingRegionalTownHallGroupAdjuster(0.05),
                new CapRegionalTownHallGroupAdjuster(14862.0),
            },
            new List<string>(),
            2.2,
            22895.8);
    }

    public double Compute(RegionalTownHallGroup item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalTownHallGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
