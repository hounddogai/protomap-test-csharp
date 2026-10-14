namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the desk audit calculation.</summary>
public interface IDeskAuditModifier
{
    string Describe();
    double Adjust(DeskAudit item, double valueValue);
}

public sealed record CapDeskAuditAdjuster(double Cap) : IDeskAuditModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeskAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DeskAuditMinimumOffsetAdjuster(double Weight) : IDeskAuditModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(DeskAudit item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record FlaggedDeskAuditAdjuster(Registry Flags, string Flag, double Percent) : IDeskAuditModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitDeskAuditAdjuster(Dictionary<string, double> Factors) : IDeskAuditModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(DeskAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the desk audit steps in order and keeps a trace of each.</summary>
public sealed class DeskAuditEngine(List<IDeskAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskAuditEngine DefaultDeskAuditEngine(Registry flags)
    {
        return new DeskAuditEngine(
            new List<IDeskAuditModifier>
            {
                new CapDeskAuditAdjuster(34153.0),
                new DeskAuditMinimumOffsetAdjuster(0.71),
                new FlaggedDeskAuditAdjuster(flags, "desk-audit-pending", 6.6),
                new UnitDeskAuditAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.82, ["retail"] = 1.28, ["north"] = 0.93 }),
            },
            new List<string>(),
            4.2,
            14171.9);
    }

    public double Run(DeskAudit item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DeskAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
