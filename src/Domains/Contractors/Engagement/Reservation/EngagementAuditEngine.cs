namespace Acme.Workforce.Contractors.Engagement.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement audit calculation.</summary>
public interface IEngagementAuditFactor
{
    string Describe();
    double Adjust(EngagementAudit item, double valueValue);
}

public sealed record PercentEngagementAuditAdjuster(double Percent) : IEngagementAuditFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EngagementAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumEngagementAuditAdjuster(double Minimum) : IEngagementAuditFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedEngagementAuditAdjuster(Registry Flags, string Flag, double Percent) : IEngagementAuditFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameEngagementAuditAdjuster(Dictionary<string, double> Factors) : IEngagementAuditFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(EngagementAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the engagement audit steps in order and keeps a trace of each.</summary>
public sealed class EngagementAuditEngine(List<IEngagementAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementAuditEngine DefaultEngagementAuditEngine(Registry flags)
    {
        return new EngagementAuditEngine(
            new List<IEngagementAuditFactor>
            {
                new PercentEngagementAuditAdjuster(20.1),
                new MinimumEngagementAuditAdjuster(33.7),
                new FlaggedEngagementAuditAdjuster(flags, "engagement-audit-batched", -2.5),
                new NameEngagementAuditAdjuster(new Dictionary<string, double> { ["south"] = 1.05 }),
            },
            new List<string>(),
            3.6,
            23214.4);
    }

    public double Evaluate(EngagementAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<EngagementAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
