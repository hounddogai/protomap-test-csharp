namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment target is in its life cycle.</summary>
public enum AdjustmentTargetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A adjustment target. Its code is unique within a tenant.</summary>
public sealed class AdjustmentTarget(string code, string label, string department, int createdAt, string region, int capacity, int durationMinutes, string notes, string phone, double ceiling, bool archived, AdjustmentTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public int CreatedAt { get; set; } = createdAt;
    public string Region { get; set; } = region;
    public int Capacity { get; set; } = capacity;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Notes { get; set; } = notes;
    public string Phone { get; set; } = phone;
    public double Ceiling { get; set; } = ceiling;
    public bool Archived { get; set; } = archived;
    public AdjustmentTargetStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment target in status open.</summary>
    public static AdjustmentTarget NewAdjustmentTarget(string code, string label)
    {
        return new AdjustmentTarget(code, label, "", 0, "", 0, 0, "", "", 0.0, false, AdjustmentTargetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentTargetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentTargetStatus next)
    {
        switch (Status)
        {
            case AdjustmentTargetStatus.Open:
                switch (next)
                {
                    case AdjustmentTargetStatus.InReview:
                        return true;
                }
                return false;
            case AdjustmentTargetStatus.InReview:
                switch (next)
                {
                    case AdjustmentTargetStatus.Reopened:
                        return true;
                }
                return false;
            case AdjustmentTargetStatus.Resolved:
                switch (next)
                {
                    case AdjustmentTargetStatus.Reopened:
                    case AdjustmentTargetStatus.Archived:
                        return true;
                }
                return false;
            case AdjustmentTargetStatus.Reopened:
                switch (next)
                {
                    case AdjustmentTargetStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetLoad()
    {
        return Math.Round(Ceiling - (double)CreatedAt, 2);
    }

    public AdjustmentTarget Copy()
    {
        return new AdjustmentTarget(
            Code,
            Label,
            Department,
            CreatedAt,
            Region,
            Capacity,
            DurationMinutes,
            Notes,
            Phone,
            Ceiling,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
