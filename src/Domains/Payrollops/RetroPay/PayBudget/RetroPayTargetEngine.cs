namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the retro pay target calculation.</summary>
public interface IRetroPayTargetAdjuster
{
    string Describe();
    double Adjust(RetroPayTarget item, double valueValue);
}

public sealed record TieredRetroPayTargetAdjuster(List<double> Bounds, List<double> Rates) : IRetroPayTargetAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RetroPayTarget item, double valueValue)
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

public sealed record RoundingRetroPayTargetAdjuster(double Step) : IRetroPayTargetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RetroPayTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRetroPayTargetAdjuster(double Percent) : IRetroPayTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RetroPayTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRetroPayTargetAdjuster(double Cap) : IRetroPayTargetAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RetroPayTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRetroPayTargetAdjuster(Registry Flags, string Flag, double Percent) : IRetroPayTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RetroPayTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the retro pay target steps in order and keeps a trace of each.</summary>
public sealed class RetroPayTargetEngine(List<IRetroPayTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayTargetEngine DefaultRetroPayTargetEngine(Registry flags)
    {
        return new RetroPayTargetEngine(
            new List<IRetroPayTargetAdjuster>
            {
                new TieredRetroPayTargetAdjuster(
                    new List<double> { 1274.0, 7360.0, 7619.0, 8564.0 },
                    new List<double> { 1.05, 1.07, 1.03, 0.96 }),
                new RoundingRetroPayTargetAdjuster(0.1),
                new PercentRetroPayTargetAdjuster(-13.2),
                new CapRetroPayTargetAdjuster(47338.0),
                new FlaggedRetroPayTargetAdjuster(flags, "retro-pay-target-scheduled", -7.8),
            },
            new List<string>(),
            3.3,
            80148.8);
    }

    public double Price(RetroPayTarget item)
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
