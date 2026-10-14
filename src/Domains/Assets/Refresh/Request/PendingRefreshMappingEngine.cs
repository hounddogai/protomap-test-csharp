namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending refresh mapping calculation.</summary>
public interface IPendingRefreshMappingAdjuster
{
    string Describe();
    double Adjust(PendingRefreshMapping item, double valueValue);
}

public sealed record TieredPendingRefreshMappingAdjuster(List<double> Bounds, List<double> Rates) : IPendingRefreshMappingAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingRefreshMapping item, double valueValue)
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

public sealed record RoundingPendingRefreshMappingAdjuster(double Step) : IPendingRefreshMappingAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingRefreshMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelPendingRefreshMappingAdjuster(Dictionary<string, double> Factors) : IPendingRefreshMappingAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PendingRefreshMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPendingRefreshMappingAdjuster(double Minimum) : IPendingRefreshMappingAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingRefreshMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPendingRefreshMappingAdjuster(double Cap) : IPendingRefreshMappingAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingRefreshMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pending refresh mapping steps in order and keeps a trace of each.</summary>
public sealed class PendingRefreshMappingEngine(List<IPendingRefreshMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingRefreshMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingRefreshMappingEngine DefaultPendingRefreshMappingEngine()
    {
        return new PendingRefreshMappingEngine(
            new List<IPendingRefreshMappingAdjuster>
            {
                new TieredPendingRefreshMappingAdjuster(
                    new List<double> { 1093.0, 3204.0, 4026.0, 4326.0 },
                    new List<double> { 1.18, 0.87, 1.16, 1.13 }),
                new RoundingPendingRefreshMappingAdjuster(0.25),
                new LabelPendingRefreshMappingAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.14, ["north"] = 1.03, ["central"] = 0.75 }),
                new MinimumPendingRefreshMappingAdjuster(43.0),
                new CapPendingRefreshMappingAdjuster(29199.0),
            },
            new List<string>(),
            4.1,
            89412.1);
    }

    public double Price(PendingRefreshMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingRefreshMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
