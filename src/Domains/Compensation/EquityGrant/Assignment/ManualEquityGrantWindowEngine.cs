namespace Acme.Workforce.Compensation.EquityGrant.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual equity grant window calculation.</summary>
public interface IManualEquityGrantWindowAdjuster
{
    string Describe();
    double Adjust(ManualEquityGrantWindow item, double valueValue);
}

public sealed record ManualEquityGrantWindowLeaveDaysOffsetAdjuster(double Weight) : IManualEquityGrantWindowAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(ManualEquityGrantWindow item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record CapManualEquityGrantWindowAdjuster(double Cap) : IManualEquityGrantWindowAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualEquityGrantWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamManualEquityGrantWindowAdjuster(Dictionary<string, double> Factors) : IManualEquityGrantWindowAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(ManualEquityGrantWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedManualEquityGrantWindowAdjuster(Registry Flags, string Flag, double Percent) : IManualEquityGrantWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualEquityGrantWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingManualEquityGrantWindowAdjuster(double Step) : IManualEquityGrantWindowAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualEquityGrantWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredManualEquityGrantWindowAdjuster(List<double> Bounds, List<double> Rates) : IManualEquityGrantWindowAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualEquityGrantWindow item, double valueValue)
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

/// <summary>Runs the manual equity grant window steps in order and keeps a trace of each.</summary>
public sealed class ManualEquityGrantWindowEngine(List<IManualEquityGrantWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualEquityGrantWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualEquityGrantWindowEngine DefaultManualEquityGrantWindowEngine(Registry flags)
    {
        return new ManualEquityGrantWindowEngine(
            new List<IManualEquityGrantWindowAdjuster>
            {
                new ManualEquityGrantWindowLeaveDaysOffsetAdjuster(1.1),
                new CapManualEquityGrantWindowAdjuster(4322.0),
                new TeamManualEquityGrantWindowAdjuster(new Dictionary<string, double> { ["east"] = 0.97 }),
                new FlaggedManualEquityGrantWindowAdjuster(flags, "manual-equity-grant-window-weighted", 3.0),
                new RoundingManualEquityGrantWindowAdjuster(1.0),
                new TieredManualEquityGrantWindowAdjuster(
                    new List<double> { 7407.0, 9902.0 },
                    new List<double> { 0.88, 1.2 }),
            },
            new List<string>(),
            2.1,
            70198.8);
    }

    public double Run(ManualEquityGrantWindow item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ManualEquityGrantWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
