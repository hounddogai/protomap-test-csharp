namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending audit threshold is in its life cycle.</summary>
public enum PendingAuditThresholdStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One pending audit threshold, with what the planners need to act on it.</summary>
public sealed class PendingAuditThreshold(string code, string title, string unit, string externalRef, string phone, string label, double score, double rate, string currency, string team, int updatedAt, PendingAuditThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public string ExternalRef { get; set; } = externalRef;
    public string Phone { get; set; } = phone;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public double Rate { get; set; } = rate;
    public string Currency { get; set; } = currency;
    public string Team { get; set; } = team;
    public int UpdatedAt { get; set; } = updatedAt;
    public PendingAuditThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a pending audit threshold in status idle.</summary>
    public static PendingAuditThreshold NewPendingAuditThreshold(string code, string title)
    {
        return new PendingAuditThreshold(
            code,
            title,
            "",
            "",
            "",
            "",
            0.0,
            0.0,
            "",
            "",
            0,
            PendingAuditThresholdStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAuditThresholdStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAuditThresholdStatus next)
    {
        switch (Status)
        {
            case PendingAuditThresholdStatus.Idle:
                return next == PendingAuditThresholdStatus.Draining || next == PendingAuditThresholdStatus.Stopped;
            case PendingAuditThresholdStatus.Warming:
                switch (next)
                {
                    case PendingAuditThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case PendingAuditThresholdStatus.Ready:
                return next == PendingAuditThresholdStatus.Draining || next == PendingAuditThresholdStatus.Stopped;
            case PendingAuditThresholdStatus.Draining:
                return next == PendingAuditThresholdStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PendingAuditThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending audit threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"pending audit threshold"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Score * (double)UpdatedAt, 2);
    }

    public PendingAuditThreshold Copy()
    {
        return new PendingAuditThreshold(
            Code,
            Title,
            Unit,
            ExternalRef,
            Phone,
            Label,
            Score,
            Rate,
            Currency,
            Team,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
