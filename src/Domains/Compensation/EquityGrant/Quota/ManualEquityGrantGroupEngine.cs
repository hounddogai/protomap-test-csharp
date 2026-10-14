namespace Acme.Workforce.Compensation.EquityGrant.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual equity grant group calculation.</summary>
public interface IManualEquityGrantGroupModifier
{
    string Describe();
    double Adjust(ManualEquityGrantGroup item, double valueValue);
}

public sealed record MinimumManualEquityGrantGroupAdjuster(double Minimum) : IManualEquityGrantGroupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualEquityGrantGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedManualEquityGrantGroupAdjuster(Registry Flags, string Flag, double Percent) : IManualEquityGrantGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualEquityGrantGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentManualEquityGrantGroupAdjuster(double Percent) : IManualEquityGrantGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualEquityGrantGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualEquityGrantGroupAdjuster(double Cap) : IManualEquityGrantGroupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualEquityGrantGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingManualEquityGrantGroupAdjuster(double Step) : IManualEquityGrantGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualEquityGrantGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the manual equity grant group steps in order and keeps a trace of each.</summary>
public sealed class ManualEquityGrantGroupEngine(List<IManualEquityGrantGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualEquityGrantGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualEquityGrantGroupEngine DefaultManualEquityGrantGroupEngine(Registry flags)
    {
        return new ManualEquityGrantGroupEngine(
            new List<IManualEquityGrantGroupModifier>
            {
                new MinimumManualEquityGrantGroupAdjuster(4.8),
                new FlaggedManualEquityGrantGroupAdjuster(flags, "manual-equity-grant-group-adaptive", 2.0),
                new PercentManualEquityGrantGroupAdjuster(-18.8),
                new CapManualEquityGrantGroupAdjuster(43900.0),
                new RoundingManualEquityGrantGroupAdjuster(5.0),
            },
            new List<string>(),
            1.0,
            88260.0);
    }

    public double Evaluate(ManualEquityGrantGroup item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ManualEquityGrantGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
