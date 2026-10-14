namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pulse survey metric is in its life cycle.</summary>
public enum ScheduledPulseSurveyMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A scheduled pulse survey metric. Its code is unique within a tenant.</summary>
public sealed class ScheduledPulseSurveyMetric(string code, string title, int priority, string unit, double score, int durationMinutes, int attempts, string location, string category, ScheduledPulseSurveyMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public string Unit { get; set; } = unit;
    public double Score { get; set; } = score;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Attempts { get; set; } = attempts;
    public string Location { get; set; } = location;
    public string Category { get; set; } = category;
    public ScheduledPulseSurveyMetricStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pulse survey metric in status idle.</summary>
    public static ScheduledPulseSurveyMetric NewScheduledPulseSurveyMetric(string code, string title)
    {
        return new ScheduledPulseSurveyMetric(
            code,
            title,
            0,
            "",
            0.0,
            0,
            0,
            "",
            "",
            ScheduledPulseSurveyMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPulseSurveyMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPulseSurveyMetricStatus next)
    {
        switch (Status)
        {
            case ScheduledPulseSurveyMetricStatus.Idle:
                return next == ScheduledPulseSurveyMetricStatus.Ready;
            case ScheduledPulseSurveyMetricStatus.Warming:
                return next == ScheduledPulseSurveyMetricStatus.Draining;
            case ScheduledPulseSurveyMetricStatus.Ready:
                switch (next)
                {
                    case ScheduledPulseSurveyMetricStatus.Draining:
                    case ScheduledPulseSurveyMetricStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledPulseSurveyMetricStatus.Draining:
                return next == ScheduledPulseSurveyMetricStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScheduledPulseSurveyMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pulse survey metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled pulse survey metric"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)Attempts + (double)DurationMinutes, 2);
    }

    public ScheduledPulseSurveyMetric Copy()
    {
        return new ScheduledPulseSurveyMetric(
            Code,
            Title,
            Priority,
            Unit,
            Score,
            DurationMinutes,
            Attempts,
            Location,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
