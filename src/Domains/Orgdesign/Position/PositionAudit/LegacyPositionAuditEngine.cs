namespace Acme.Workforce.Orgdesign.Position.PositionAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy position audit calculation.</summary>
public interface ILegacyPositionAuditAdjuster
{
    string Describe();
    double Adjust(LegacyPositionAudit item, double valueValue);
}

public sealed record TieredLegacyPositionAuditAdjuster(List<double> Bounds, List<double> Rates) : ILegacyPositionAuditAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyPositionAudit item, double valueValue)
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

public sealed record CapLegacyPositionAuditAdjuster(double Cap) : ILegacyPositionAuditAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyPositionAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLegacyPositionAuditAdjuster(double Step) : ILegacyPositionAuditAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyPositionAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LegacyPositionAuditLeaveDaysOffsetAdjuster(double Weight) : ILegacyPositionAuditAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(LegacyPositionAudit item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record FlaggedLegacyPositionAuditAdjuster(Registry Flags, string Flag, double Percent) : ILegacyPositionAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyPositionAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyPositionAuditAdjuster(double Minimum) : ILegacyPositionAuditAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyPositionAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy position audit steps in order and keeps a trace of each.</summary>
public sealed class LegacyPositionAuditEngine(List<ILegacyPositionAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPositionAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPositionAuditEngine DefaultLegacyPositionAuditEngine(Registry flags)
    {
        return new LegacyPositionAuditEngine(
            new List<ILegacyPositionAuditAdjuster>
            {
                new TieredLegacyPositionAuditAdjuster(
                    new List<double> { 3648.0, 8855.0 },
                    new List<double> { 0.88, 0.86 }),
                new CapLegacyPositionAuditAdjuster(49182.0),
                new RoundingLegacyPositionAuditAdjuster(0.5),
                new LegacyPositionAuditLeaveDaysOffsetAdjuster(1.8),
                new FlaggedLegacyPositionAuditAdjuster(flags, "legacy-position-audit-pending", 2.3),
                new MinimumLegacyPositionAuditAdjuster(29.1),
            },
            new List<string>(),
            1.8,
            73585.4);
    }

    public double Evaluate(LegacyPositionAudit item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPositionAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
