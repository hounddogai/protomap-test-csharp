namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive accrual metric is in its life cycle.</summary>
public enum AdaptiveAccrualMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a adaptive accrual metric and its current status.</summary>
public sealed class AdaptiveAccrualMetric(string code, string title, string externalRef, int priority, int retries, double threshold, string description, int startsAt, string summary, string contactEmail, AdaptiveAccrualMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public int Priority { get; set; } = priority;
    public int Retries { get; set; } = retries;
    public double Threshold { get; set; } = threshold;
    public string Description { get; set; } = description;
    public int StartsAt { get; set; } = startsAt;
    public string Summary { get; set; } = summary;
    public string ContactEmail { get; set; } = contactEmail;
    public AdaptiveAccrualMetricStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive accrual metric in status requested.</summary>
    public static AdaptiveAccrualMetric NewAdaptiveAccrualMetric(string code, string title)
    {
        return new AdaptiveAccrualMetric(
            code,
            title,
            "",
            0,
            0,
            0.0,
            "",
            0,
            "",
            "",
            AdaptiveAccrualMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAccrualMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAccrualMetricStatus next)
    {
        switch (Status)
        {
            case AdaptiveAccrualMetricStatus.Requested:
                return next == AdaptiveAccrualMetricStatus.Expired;
            case AdaptiveAccrualMetricStatus.Granted:
                return next == AdaptiveAccrualMetricStatus.Expired;
            case AdaptiveAccrualMetricStatus.Revoked:
                switch (next)
                {
                    case AdaptiveAccrualMetricStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveAccrualMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive accrual metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Retries * Threshold, 2);
    }

    public AdaptiveAccrualMetric Copy()
    {
        return new AdaptiveAccrualMetric(
            Code,
            Title,
            ExternalRef,
            Priority,
            Retries,
            Threshold,
            Description,
            StartsAt,
            Summary,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
