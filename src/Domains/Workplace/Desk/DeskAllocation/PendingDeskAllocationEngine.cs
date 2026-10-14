namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending desk allocation calculation.</summary>
public interface IPendingDeskAllocationFactor
{
    string Describe();
    double Adjust(PendingDeskAllocation item, double valueValue);
}

public sealed record PendingDeskAllocationRetriesOffsetAdjuster(double Weight) : IPendingDeskAllocationFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(PendingDeskAllocation item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record RoundingPendingDeskAllocationAdjuster(double Step) : IPendingDeskAllocationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingDeskAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamPendingDeskAllocationAdjuster(Dictionary<string, double> Factors) : IPendingDeskAllocationFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(PendingDeskAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPendingDeskAllocationAdjuster(double Minimum) : IPendingDeskAllocationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingDeskAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pending desk allocation steps in order and keeps a trace of each.</summary>
public sealed class PendingDeskAllocationEngine(List<IPendingDeskAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingDeskAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingDeskAllocationEngine DefaultPendingDeskAllocationEngine()
    {
        return new PendingDeskAllocationEngine(
            new List<IPendingDeskAllocationFactor>
            {
                new PendingDeskAllocationRetriesOffsetAdjuster(0.16),
                new RoundingPendingDeskAllocationAdjuster(0.25),
                new TeamPendingDeskAllocationAdjuster(new Dictionary<string, double> { ["retail"] = 1.19 }),
                new MinimumPendingDeskAllocationAdjuster(43.2),
            },
            new List<string>(),
            2.8,
            35121.2);
    }

    public double Price(PendingDeskAllocation item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PendingDeskAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
