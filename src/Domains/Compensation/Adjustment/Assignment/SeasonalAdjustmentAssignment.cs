namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal adjustment assignment is in its life cycle.</summary>
public enum SeasonalAdjustmentAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A seasonal adjustment assignment. Its code is unique within a tenant.</summary>
public sealed class SeasonalAdjustmentAssignment(string code, string title, string costCenter, string managerId, int createdAt, double discount, double ceiling, string notes, string employeeId, bool archived, SeasonalAdjustmentAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public string ManagerId { get; set; } = managerId;
    public int CreatedAt { get; set; } = createdAt;
    public double Discount { get; set; } = discount;
    public double Ceiling { get; set; } = ceiling;
    public string Notes { get; set; } = notes;
    public string EmployeeId { get; set; } = employeeId;
    public bool Archived { get; set; } = archived;
    public SeasonalAdjustmentAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal adjustment assignment in status open.</summary>
    public static SeasonalAdjustmentAssignment NewSeasonalAdjustmentAssignment(string code, string title)
    {
        return new SeasonalAdjustmentAssignment(
            code,
            title,
            "",
            "",
            0,
            0.0,
            0.0,
            "",
            "",
            false,
            SeasonalAdjustmentAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAdjustmentAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAdjustmentAssignmentStatus next)
    {
        switch (Status)
        {
            case SeasonalAdjustmentAssignmentStatus.Open:
                return next == SeasonalAdjustmentAssignmentStatus.Resolved;
            case SeasonalAdjustmentAssignmentStatus.InReview:
                switch (next)
                {
                    case SeasonalAdjustmentAssignmentStatus.Reopened:
                    case SeasonalAdjustmentAssignmentStatus.Archived:
                        return true;
                }
                return false;
            case SeasonalAdjustmentAssignmentStatus.Resolved:
                switch (next)
                {
                    case SeasonalAdjustmentAssignmentStatus.Reopened:
                        return true;
                }
                return false;
            case SeasonalAdjustmentAssignmentStatus.Reopened:
                switch (next)
                {
                    case SeasonalAdjustmentAssignmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalAdjustmentAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal adjustment assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(Ceiling - Discount, 2);
    }

    public SeasonalAdjustmentAssignment Copy()
    {
        return new SeasonalAdjustmentAssignment(
            Code,
            Title,
            CostCenter,
            ManagerId,
            CreatedAt,
            Discount,
            Ceiling,
            Notes,
            EmployeeId,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
