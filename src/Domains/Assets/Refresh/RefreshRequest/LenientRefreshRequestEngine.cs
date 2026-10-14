namespace Acme.Workforce.Assets.Refresh.RefreshRequest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient refresh request calculation.</summary>
public interface ILenientRefreshRequestFactor
{
    string Describe();
    double Adjust(LenientRefreshRequest item, double valueValue);
}

public sealed record MinimumLenientRefreshRequestAdjuster(double Minimum) : ILenientRefreshRequestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientRefreshRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLenientRefreshRequestAdjuster(Registry Flags, string Flag, double Percent) : ILenientRefreshRequestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientRefreshRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientRefreshRequestHoursOffsetAdjuster(double Weight) : ILenientRefreshRequestFactor
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(LenientRefreshRequest item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record RoundingLenientRefreshRequestAdjuster(double Step) : ILenientRefreshRequestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientRefreshRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLenientRefreshRequestAdjuster(double Cap) : ILenientRefreshRequestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientRefreshRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLenientRefreshRequestAdjuster(List<double> Bounds, List<double> Rates) : ILenientRefreshRequestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientRefreshRequest item, double valueValue)
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

/// <summary>Runs the lenient refresh request steps in order and keeps a trace of each.</summary>
public sealed class LenientRefreshRequestEngine(List<ILenientRefreshRequestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientRefreshRequestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientRefreshRequestEngine DefaultLenientRefreshRequestEngine(Registry flags)
    {
        return new LenientRefreshRequestEngine(
            new List<ILenientRefreshRequestFactor>
            {
                new MinimumLenientRefreshRequestAdjuster(49.5),
                new FlaggedLenientRefreshRequestAdjuster(flags, "lenient-refresh-request-default", 7.0),
                new LenientRefreshRequestHoursOffsetAdjuster(0.4),
                new RoundingLenientRefreshRequestAdjuster(0.25),
                new CapLenientRefreshRequestAdjuster(19198.0),
                new TieredLenientRefreshRequestAdjuster(
                    new List<double> { 5847.0, 6872.0, 9272.0 },
                    new List<double> { 0.95, 1.11, 1.07 }),
            },
            new List<string>(),
            2.4,
            71341.2);
    }

    public double Run(LenientRefreshRequest item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LenientRefreshRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
