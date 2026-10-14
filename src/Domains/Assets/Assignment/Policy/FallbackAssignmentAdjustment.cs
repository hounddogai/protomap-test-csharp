namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback assignment adjustment is in its life cycle.</summary>
public enum FallbackAssignmentAdjustmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A fallback assignment adjustment. Its code is unique within a tenant.</summary>
public sealed class FallbackAssignmentAdjustment(string code, string title, string managerId, string badge, int expiresAt, string jobCode, int leaveDays, string notes, string name, double rating, double score, FallbackAssignmentAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public string Badge { get; set; } = badge;
    public int ExpiresAt { get; set; } = expiresAt;
    public string JobCode { get; set; } = jobCode;
    public int LeaveDays { get; set; } = leaveDays;
    public string Notes { get; set; } = notes;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public double Score { get; set; } = score;
    public FallbackAssignmentAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a fallback assignment adjustment in status planned.</summary>
    public static FallbackAssignmentAdjustment NewFallbackAssignmentAdjustment(string code, string title)
    {
        return new FallbackAssignmentAdjustment(
            code,
            title,
            "",
            "",
            0,
            "",
            0,
            "",
            "",
            0.0,
            0.0,
            FallbackAssignmentAdjustmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAssignmentAdjustmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAssignmentAdjustmentStatus next)
    {
        switch (Status)
        {
            case FallbackAssignmentAdjustmentStatus.Planned:
                switch (next)
                {
                    case FallbackAssignmentAdjustmentStatus.Completed:
                        return true;
                }
                return false;
            case FallbackAssignmentAdjustmentStatus.Scheduled:
                switch (next)
                {
                    case FallbackAssignmentAdjustmentStatus.InProgress:
                    case FallbackAssignmentAdjustmentStatus.Completed:
                        return true;
                }
                return false;
            case FallbackAssignmentAdjustmentStatus.InProgress:
                return next == FallbackAssignmentAdjustmentStatus.Completed || next == FallbackAssignmentAdjustmentStatus.Cancelled;
            case FallbackAssignmentAdjustmentStatus.Completed:
                return next == FallbackAssignmentAdjustmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FallbackAssignmentAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback assignment adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedValue()
    {
        return Math.Round(Score + (double)ExpiresAt, 2);
    }

    public FallbackAssignmentAdjustment Copy()
    {
        return new FallbackAssignmentAdjustment(
            Code,
            Title,
            ManagerId,
            Badge,
            ExpiresAt,
            JobCode,
            LeaveDays,
            Notes,
            Name,
            Rating,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
