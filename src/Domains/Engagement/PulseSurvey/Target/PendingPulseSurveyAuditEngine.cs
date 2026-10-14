namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending pulse survey audit calculation.</summary>
public interface IPendingPulseSurveyAuditStep
{
    string Describe();
    double Adjust(PendingPulseSurveyAudit item, double valueValue);
}

public sealed record LocalePendingPulseSurveyAuditAdjuster(Dictionary<string, double> Factors) : IPendingPulseSurveyAuditStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(PendingPulseSurveyAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPendingPulseSurveyAuditAdjuster(Registry Flags, string Flag, double Percent) : IPendingPulseSurveyAuditStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingPulseSurveyAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPendingPulseSurveyAuditAdjuster(double Cap) : IPendingPulseSurveyAuditStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingPulseSurveyAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPendingPulseSurveyAuditAdjuster(List<double> Bounds, List<double> Rates) : IPendingPulseSurveyAuditStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingPulseSurveyAudit item, double valueValue)
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

public sealed record MinimumPendingPulseSurveyAuditAdjuster(double Minimum) : IPendingPulseSurveyAuditStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingPulseSurveyAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pending pulse survey audit steps in order and keeps a trace of each.</summary>
public sealed class PendingPulseSurveyAuditEngine(List<IPendingPulseSurveyAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingPulseSurveyAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingPulseSurveyAuditEngine DefaultPendingPulseSurveyAuditEngine(Registry flags)
    {
        return new PendingPulseSurveyAuditEngine(
            new List<IPendingPulseSurveyAuditStep>
            {
                new LocalePendingPulseSurveyAuditAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.72, ["central"] = 0.85, ["retail"] = 0.83 }),
                new FlaggedPendingPulseSurveyAuditAdjuster(flags, "pending-pulse-survey-audit-scheduled", 0.7),
                new CapPendingPulseSurveyAuditAdjuster(665.0),
                new TieredPendingPulseSurveyAuditAdjuster(
                    new List<double> { 2112.0, 5750.0 },
                    new List<double> { 1.02, 0.87 }),
                new MinimumPendingPulseSurveyAuditAdjuster(47.4),
            },
            new List<string>(),
            0.9,
            22567.4);
    }

    public double Price(PendingPulseSurveyAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingPulseSurveyAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
