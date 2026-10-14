namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending warranty group calculation.</summary>
public interface IPendingWarrantyGroupModifier
{
    string Describe();
    double Adjust(PendingWarrantyGroup item, double valueValue);
}

public sealed record FlaggedPendingWarrantyGroupAdjuster(Registry Flags, string Flag, double Percent) : IPendingWarrantyGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingWarrantyGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPendingWarrantyGroupAdjuster(double Cap) : IPendingWarrantyGroupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingWarrantyGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPendingWarrantyGroupAdjuster(double Percent) : IPendingWarrantyGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingWarrantyGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending warranty group steps in order and keeps a trace of each.</summary>
public sealed class PendingWarrantyGroupEngine(List<IPendingWarrantyGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingWarrantyGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingWarrantyGroupEngine DefaultPendingWarrantyGroupEngine(Registry flags)
    {
        return new PendingWarrantyGroupEngine(
            new List<IPendingWarrantyGroupModifier>
            {
                new FlaggedPendingWarrantyGroupAdjuster(flags, "pending-warranty-group-pending", -3.8),
                new CapPendingWarrantyGroupAdjuster(19903.0),
                new PercentPendingWarrantyGroupAdjuster(-8.7),
            },
            new List<string>(),
            1.6,
            96570.4);
    }

    public double Evaluate(PendingWarrantyGroup item)
    {
        var current = item.ProjectedLoad();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<PendingWarrantyGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
