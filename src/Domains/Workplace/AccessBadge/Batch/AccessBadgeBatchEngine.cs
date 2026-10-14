namespace Acme.Workforce.Workplace.AccessBadge.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge batch calculation.</summary>
public interface IAccessBadgeBatchAdjuster
{
    string Describe();
    double Adjust(AccessBadgeBatch item, double valueValue);
}

public sealed record FlaggedAccessBadgeBatchAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAccessBadgeBatchAdjuster(double Step) : IAccessBadgeBatchAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAccessBadgeBatchAdjuster(double Cap) : IAccessBadgeBatchAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccessBadgeBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AccessBadgeBatchRateOffsetAdjuster(double Weight) : IAccessBadgeBatchAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(AccessBadgeBatch item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the access badge batch steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeBatchEngine(List<IAccessBadgeBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeBatchEngine DefaultAccessBadgeBatchEngine(Registry flags)
    {
        return new AccessBadgeBatchEngine(
            new List<IAccessBadgeBatchAdjuster>
            {
                new FlaggedAccessBadgeBatchAdjuster(flags, "access-badge-batch-batched", 5.8),
                new RoundingAccessBadgeBatchAdjuster(0.05),
                new CapAccessBadgeBatchAdjuster(8783.0),
                new AccessBadgeBatchRateOffsetAdjuster(1.69),
            },
            new List<string>(),
            1.9,
            29971.8);
    }

    public double Compute(AccessBadgeBatch item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
