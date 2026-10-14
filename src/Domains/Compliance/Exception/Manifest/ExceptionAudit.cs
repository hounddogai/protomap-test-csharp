namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception audit is in its life cycle.</summary>
public enum ExceptionAuditStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One exception audit, with what the planners need to act on it.</summary>
public sealed class ExceptionAudit(string code, string label, int updatedAt, string location, string region, string title, int durationMinutes, string ownerId, int priority, double minimum, double discount, ExceptionAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Location { get; set; } = location;
    public string Region { get; set; } = region;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string OwnerId { get; set; } = ownerId;
    public int Priority { get; set; } = priority;
    public double Minimum { get; set; } = minimum;
    public double Discount { get; set; } = discount;
    public ExceptionAuditStatus Status { get; set; } = status;

    /// <summary>Starts a exception audit in status idle.</summary>
    public static ExceptionAudit NewExceptionAudit(string code, string label)
    {
        return new ExceptionAudit(code, label, 0, "", "", "", 0, "", 0, 0.0, 0.0, ExceptionAuditStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionAuditStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionAuditStatus next)
    {
        switch (Status)
        {
            case ExceptionAuditStatus.Idle:
                return next == ExceptionAuditStatus.Warming || next == ExceptionAuditStatus.Ready;
            case ExceptionAuditStatus.Warming:
                switch (next)
                {
                    case ExceptionAuditStatus.Ready:
                        return true;
                }
                return false;
            case ExceptionAuditStatus.Ready:
                return next == ExceptionAuditStatus.Stopped;
            case ExceptionAuditStatus.Draining:
                return next == ExceptionAuditStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ExceptionAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"exception audit"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Priority + Minimum, 2);
    }

    public ExceptionAudit Copy()
    {
        return new ExceptionAudit(
            Code,
            Label,
            UpdatedAt,
            Location,
            Region,
            Title,
            DurationMinutes,
            OwnerId,
            Priority,
            Minimum,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
