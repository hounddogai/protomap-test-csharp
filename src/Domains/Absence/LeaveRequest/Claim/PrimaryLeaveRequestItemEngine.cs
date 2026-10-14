namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary leave request item calculation.</summary>
public interface IPrimaryLeaveRequestItemModifier
{
    string Describe();
    double Adjust(PrimaryLeaveRequestItem item, double valueValue);
}

public sealed record TieredPrimaryLeaveRequestItemAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryLeaveRequestItemModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryLeaveRequestItem item, double valueValue)
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

public sealed record NamePrimaryLeaveRequestItemAdjuster(Dictionary<string, double> Factors) : IPrimaryLeaveRequestItemModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PrimaryLeaveRequestItem item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPrimaryLeaveRequestItemAdjuster(double Percent) : IPrimaryLeaveRequestItemModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryLeaveRequestItem item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPrimaryLeaveRequestItemAdjuster(double Cap) : IPrimaryLeaveRequestItemModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryLeaveRequestItem item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPrimaryLeaveRequestItemAdjuster(double Minimum) : IPrimaryLeaveRequestItemModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryLeaveRequestItem item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the primary leave request item steps in order and keeps a trace of each.</summary>
public sealed class PrimaryLeaveRequestItemEngine(List<IPrimaryLeaveRequestItemModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryLeaveRequestItemModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryLeaveRequestItemEngine DefaultPrimaryLeaveRequestItemEngine()
    {
        return new PrimaryLeaveRequestItemEngine(
            new List<IPrimaryLeaveRequestItemModifier>
            {
                new TieredPrimaryLeaveRequestItemAdjuster(
                    new List<double> { 1114.0, 6671.0, 9783.0 },
                    new List<double> { 0.95, 1.04, 0.82 }),
                new NamePrimaryLeaveRequestItemAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.18, ["west"] = 1.22, ["online"] = 0.88 }),
                new PercentPrimaryLeaveRequestItemAdjuster(0.1),
                new CapPrimaryLeaveRequestItemAdjuster(48600.0),
                new MinimumPrimaryLeaveRequestItemAdjuster(9.1),
            },
            new List<string>(),
            3.6,
            62887.5);
    }

    public double Evaluate(PrimaryLeaveRequestItem item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryLeaveRequestItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
