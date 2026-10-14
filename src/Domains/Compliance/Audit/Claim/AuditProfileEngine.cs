namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit profile calculation.</summary>
public interface IAuditProfileModifier
{
    string Describe();
    double Adjust(AuditProfile item, double valueValue);
}

public sealed record LocaleAuditProfileAdjuster(Dictionary<string, double> Factors) : IAuditProfileModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(AuditProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAuditProfileAdjuster(Registry Flags, string Flag, double Percent) : IAuditProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAuditProfileAdjuster(double Minimum) : IAuditProfileModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AuditProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AuditProfileWindowDaysOffsetAdjuster(double Weight) : IAuditProfileModifier
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(AuditProfile item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record PercentAuditProfileAdjuster(double Percent) : IAuditProfileModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the audit profile steps in order and keeps a trace of each.</summary>
public sealed class AuditProfileEngine(List<IAuditProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditProfileEngine DefaultAuditProfileEngine(Registry flags)
    {
        return new AuditProfileEngine(
            new List<IAuditProfileModifier>
            {
                new LocaleAuditProfileAdjuster(new Dictionary<string, double> { ["central"] = 0.74 }),
                new FlaggedAuditProfileAdjuster(flags, "audit-profile-default", 2.0),
                new MinimumAuditProfileAdjuster(23.8),
                new AuditProfileWindowDaysOffsetAdjuster(0.62),
                new PercentAuditProfileAdjuster(3.3),
            },
            new List<string>(),
            1.0,
            20067.8);
    }

    public double Evaluate(AuditProfile item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AuditProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
