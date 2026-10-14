namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh window calculation.</summary>
public interface IRefreshWindowAdjuster
{
    string Describe();
    double Adjust(RefreshWindow item, double valueValue);
}

public sealed record FlaggedRefreshWindowAdjuster(Registry Flags, string Flag, double Percent) : IRefreshWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRefreshWindowAdjuster(double Step) : IRefreshWindowAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRefreshWindowAdjuster(double Cap) : IRefreshWindowAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RefreshWindowCeilingOffsetAdjuster(double Weight) : IRefreshWindowAdjuster
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(RefreshWindow item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

/// <summary>Runs the refresh window steps in order and keeps a trace of each.</summary>
public sealed class RefreshWindowEngine(List<IRefreshWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshWindowEngine DefaultRefreshWindowEngine(Registry flags)
    {
        return new RefreshWindowEngine(
            new List<IRefreshWindowAdjuster>
            {
                new FlaggedRefreshWindowAdjuster(flags, "refresh-window-global", -7.1),
                new RoundingRefreshWindowAdjuster(0.5),
                new CapRefreshWindowAdjuster(11350.0),
                new RefreshWindowCeilingOffsetAdjuster(1.87),
            },
            new List<string>(),
            3.4,
            40467.5);
    }

    public double Price(RefreshWindow item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
