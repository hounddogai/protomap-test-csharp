namespace Acme.Workforce.Engagement.PulseSurvey.Quota;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the manual pulse survey audit calculation.</summary>
public interface IManualPulseSurveyAuditAdjuster
{
    string Describe();
    double Adjust(ManualPulseSurveyAudit item, double valueValue);
}

public sealed record FlaggedManualPulseSurveyAuditAdjuster(Registry Flags, string Flag, double Percent) : IManualPulseSurveyAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualPulseSurveyAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ManualPulseSurveyAuditLeaveDaysOffsetAdjuster(double Weight) : IManualPulseSurveyAuditAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(ManualPulseSurveyAudit item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record TieredManualPulseSurveyAuditAdjuster(List<double> Bounds, List<double> Rates) : IManualPulseSurveyAuditAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualPulseSurveyAudit item, double valueValue)
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

public sealed record CapManualPulseSurveyAuditAdjuster(double Cap) : IManualPulseSurveyAuditAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualPulseSurveyAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentManualPulseSurveyAuditAdjuster(double Percent) : IManualPulseSurveyAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualPulseSurveyAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumManualPulseSurveyAuditAdjuster(double Minimum) : IManualPulseSurveyAuditAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualPulseSurveyAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the manual pulse survey audit steps in order and keeps a trace of each.</summary>
public sealed class ManualPulseSurveyAuditEngine(List<IManualPulseSurveyAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualPulseSurveyAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualPulseSurveyAuditEngine DefaultManualPulseSurveyAuditEngine(Registry flags)
    {
        return new ManualPulseSurveyAuditEngine(
            new List<IManualPulseSurveyAuditAdjuster>
            {
                new FlaggedManualPulseSurveyAuditAdjuster(flags, "manual-pulse-survey-audit-strict", -6.3),
                new ManualPulseSurveyAuditLeaveDaysOffsetAdjuster(0.69),
                new TieredManualPulseSurveyAuditAdjuster(
                    new List<double> { 3000.0, 7717.0, 9464.0 },
                    new List<double> { 0.94, 1.01, 0.89 }),
                new CapManualPulseSurveyAuditAdjuster(15737.0),
                new PercentManualPulseSurveyAuditAdjuster(-12.6),
                new MinimumManualPulseSurveyAuditAdjuster(28.5),
            },
            new List<string>(),
            0.0,
            44957.0);
    }

    public double Evaluate(ManualPulseSurveyAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualPulseSurveyAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
