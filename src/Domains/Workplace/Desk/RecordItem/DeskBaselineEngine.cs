namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk baseline calculation.</summary>
public interface IDeskBaselineFactor
{
    string Describe();
    double Adjust(DeskBaseline item, double valueValue);
}

public sealed record PercentDeskBaselineAdjuster(double Percent) : IDeskBaselineFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDeskBaselineAdjuster(double Minimum) : IDeskBaselineFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeskBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDeskBaselineAdjuster(Registry Flags, string Flag, double Percent) : IDeskBaselineFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the desk baseline steps in order and keeps a trace of each.</summary>
public sealed class DeskBaselineEngine(List<IDeskBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskBaselineEngine DefaultDeskBaselineEngine(Registry flags)
    {
        return new DeskBaselineEngine(
            new List<IDeskBaselineFactor>
            {
                new PercentDeskBaselineAdjuster(15.9),
                new MinimumDeskBaselineAdjuster(4.6),
                new FlaggedDeskBaselineAdjuster(flags, "desk-baseline-seasonal", -0.1),
            },
            new List<string>(),
            4.4,
            42571.9);
    }

    public double Compute(DeskBaseline item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DeskBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
