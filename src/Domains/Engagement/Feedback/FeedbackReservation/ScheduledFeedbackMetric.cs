namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled feedback metric is in its life cycle.</summary>
public enum ScheduledFeedbackMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A scheduled feedback metric. Its code is unique within a tenant.</summary>
public sealed class ScheduledFeedbackMetric(string code, string name, double rate, string summary, int updatedAt, string currency, double ceiling, double ratio, int version, string timezone, string source, ScheduledFeedbackMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string Summary { get; set; } = summary;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Currency { get; set; } = currency;
    public double Ceiling { get; set; } = ceiling;
    public double Ratio { get; set; } = ratio;
    public int Version { get; set; } = version;
    public string Timezone { get; set; } = timezone;
    public string Source { get; set; } = source;
    public ScheduledFeedbackMetricStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled feedback metric in status requested.</summary>
    public static ScheduledFeedbackMetric NewScheduledFeedbackMetric(string code, string name)
    {
        return new ScheduledFeedbackMetric(
            code,
            name,
            0.0,
            "",
            0,
            "",
            0.0,
            0.0,
            0,
            "",
            "",
            ScheduledFeedbackMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledFeedbackMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledFeedbackMetricStatus next)
    {
        switch (Status)
        {
            case ScheduledFeedbackMetricStatus.Requested:
                switch (next)
                {
                    case ScheduledFeedbackMetricStatus.Revoked:
                    case ScheduledFeedbackMetricStatus.Expired:
                        return true;
                }
                return false;
            case ScheduledFeedbackMetricStatus.Granted:
                switch (next)
                {
                    case ScheduledFeedbackMetricStatus.Expired:
                        return true;
                }
                return false;
            case ScheduledFeedbackMetricStatus.Revoked:
                switch (next)
                {
                    case ScheduledFeedbackMetricStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledFeedbackMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled feedback metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"scheduled feedback metric"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Ceiling + (double)UpdatedAt, 2);
    }

    public ScheduledFeedbackMetric Copy()
    {
        return new ScheduledFeedbackMetric(
            Code,
            Name,
            Rate,
            Summary,
            UpdatedAt,
            Currency,
            Ceiling,
            Ratio,
            Version,
            Timezone,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
