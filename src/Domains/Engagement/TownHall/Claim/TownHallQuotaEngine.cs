namespace Acme.Workforce.Engagement.TownHall.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall quota calculation.</summary>
public interface ITownHallQuotaStep
{
    string Describe();
    double Adjust(TownHallQuota item, double valueValue);
}

public sealed record TownHallQuotaEndsAtOffsetAdjuster(double Weight) : ITownHallQuotaStep
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(TownHallQuota item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record MinimumTownHallQuotaAdjuster(double Minimum) : ITownHallQuotaStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTownHallQuotaAdjuster(double Cap) : ITownHallQuotaStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTownHallQuotaAdjuster(List<double> Bounds, List<double> Rates) : ITownHallQuotaStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallQuota item, double valueValue)
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

/// <summary>Runs the town hall quota steps in order and keeps a trace of each.</summary>
public sealed class TownHallQuotaEngine(List<ITownHallQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallQuotaEngine DefaultTownHallQuotaEngine()
    {
        return new TownHallQuotaEngine(
            new List<ITownHallQuotaStep>
            {
                new TownHallQuotaEndsAtOffsetAdjuster(0.86),
                new MinimumTownHallQuotaAdjuster(1.1),
                new CapTownHallQuotaAdjuster(18738.0),
                new TieredTownHallQuotaAdjuster(
                    new List<double> { 1136.0, 1170.0, 3052.0 },
                    new List<double> { 1.04, 1.15, 1.07 }),
            },
            new List<string>(),
            3.2,
            13454.9);
    }

    public double Price(TownHallQuota item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
