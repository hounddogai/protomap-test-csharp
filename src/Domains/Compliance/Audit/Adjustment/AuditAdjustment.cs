namespace Acme.Workforce.Compliance.Audit.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit adjustment is in its life cycle.</summary>
public enum AuditAdjustmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a audit adjustment and its current status.</summary>
public sealed class AuditAdjustment(string code, string title, int priority, double floorValue, int revision, string label, string costCenter, string channel, string region, string jobCode, int endsAt, AuditAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public double FloorValue { get; set; } = floorValue;
    public int Revision { get; set; } = revision;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public string Channel { get; set; } = channel;
    public string Region { get; set; } = region;
    public string JobCode { get; set; } = jobCode;
    public int EndsAt { get; set; } = endsAt;
    public AuditAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a audit adjustment in status open.</summary>
    public static AuditAdjustment NewAuditAdjustment(string code, string title)
    {
        return new AuditAdjustment(code, title, 0, 0.0, 0, "", "", "", "", "", 0, AuditAdjustmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditAdjustmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditAdjustmentStatus next)
    {
        switch (Status)
        {
            case AuditAdjustmentStatus.Open:
                switch (next)
                {
                    case AuditAdjustmentStatus.InReview:
                    case AuditAdjustmentStatus.Reopened:
                        return true;
                }
                return false;
            case AuditAdjustmentStatus.InReview:
                return next == AuditAdjustmentStatus.Archived;
            case AuditAdjustmentStatus.Resolved:
                switch (next)
                {
                    case AuditAdjustmentStatus.Reopened:
                    case AuditAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case AuditAdjustmentStatus.Reopened:
                switch (next)
                {
                    case AuditAdjustmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double NetValue()
    {
        return Math.Round((double)Priority + (double)Revision, 2);
    }

    public AuditAdjustment Copy()
    {
        return new AuditAdjustment(
            Code,
            Title,
            Priority,
            FloorValue,
            Revision,
            Label,
            CostCenter,
            Channel,
            Region,
            JobCode,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
