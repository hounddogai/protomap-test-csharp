namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling leave request batch is in its life cycle.</summary>
public enum RollingLeaveRequestBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a rolling leave request batch and its current status.</summary>
public sealed class RollingLeaveRequestBatch(string code, string label, double budget, string managerId, string badge, double hours, double weight, double floorValue, int createdAt, string unit, RollingLeaveRequestBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string ManagerId { get; set; } = managerId;
    public string Badge { get; set; } = badge;
    public double Hours { get; set; } = hours;
    public double Weight { get; set; } = weight;
    public double FloorValue { get; set; } = floorValue;
    public int CreatedAt { get; set; } = createdAt;
    public string Unit { get; set; } = unit;
    public RollingLeaveRequestBatchStatus Status { get; set; } = status;

    /// <summary>Starts a rolling leave request batch in status open.</summary>
    public static RollingLeaveRequestBatch NewRollingLeaveRequestBatch(string code, string label)
    {
        return new RollingLeaveRequestBatch(
            code,
            label,
            0.0,
            "",
            "",
            0.0,
            0.0,
            0.0,
            0,
            "",
            RollingLeaveRequestBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingLeaveRequestBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingLeaveRequestBatchStatus next)
    {
        switch (Status)
        {
            case RollingLeaveRequestBatchStatus.Open:
                switch (next)
                {
                    case RollingLeaveRequestBatchStatus.Resolved:
                        return true;
                }
                return false;
            case RollingLeaveRequestBatchStatus.InReview:
                return next == RollingLeaveRequestBatchStatus.Resolved || next == RollingLeaveRequestBatchStatus.Archived;
            case RollingLeaveRequestBatchStatus.Resolved:
                switch (next)
                {
                    case RollingLeaveRequestBatchStatus.Reopened:
                    case RollingLeaveRequestBatchStatus.Archived:
                        return true;
                }
                return false;
            case RollingLeaveRequestBatchStatus.Reopened:
                switch (next)
                {
                    case RollingLeaveRequestBatchStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingLeaveRequestBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling leave request batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Weight + FloorValue, 2);
    }

    public RollingLeaveRequestBatch Copy()
    {
        return new RollingLeaveRequestBatch(
            Code,
            Label,
            Budget,
            ManagerId,
            Badge,
            Hours,
            Weight,
            FloorValue,
            CreatedAt,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
