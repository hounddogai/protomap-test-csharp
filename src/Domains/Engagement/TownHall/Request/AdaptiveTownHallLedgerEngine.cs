namespace Acme.Workforce.Engagement.TownHall.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive town hall ledger calculation.</summary>
public interface IAdaptiveTownHallLedgerAdjuster
{
    string Describe();
    double Adjust(AdaptiveTownHallLedger item, double valueValue);
}

public sealed record CapAdaptiveTownHallLedgerAdjuster(double Cap) : IAdaptiveTownHallLedgerAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveTownHallLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentAdaptiveTownHallLedgerAdjuster(double Percent) : IAdaptiveTownHallLedgerAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveTownHallLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptiveTownHallLedgerAdjuster(double Minimum) : IAdaptiveTownHallLedgerAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveTownHallLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAdaptiveTownHallLedgerAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveTownHallLedgerAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveTownHallLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive town hall ledger steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveTownHallLedgerEngine(List<IAdaptiveTownHallLedgerAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveTownHallLedgerAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveTownHallLedgerEngine DefaultAdaptiveTownHallLedgerEngine(Registry flags)
    {
        return new AdaptiveTownHallLedgerEngine(
            new List<IAdaptiveTownHallLedgerAdjuster>
            {
                new CapAdaptiveTownHallLedgerAdjuster(6714.0),
                new PercentAdaptiveTownHallLedgerAdjuster(16.6),
                new MinimumAdaptiveTownHallLedgerAdjuster(33.9),
                new FlaggedAdaptiveTownHallLedgerAdjuster(flags, "adaptive-town-hall-ledger-legacy", 6.5),
            },
            new List<string>(),
            0.3,
            30479.7);
    }

    public double Price(AdaptiveTownHallLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveTownHallLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
