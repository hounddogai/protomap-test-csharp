namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request adjustment is in its life cycle.</summary>
public enum LeaveRequestAdjustmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A leave request adjustment as the leave request team keeps it.</summary>
public sealed class LeaveRequestAdjustment(string code, string title, double amount, string grade, string managerId, string country, string currency, string team, LeaveRequestAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public string Grade { get; set; } = grade;
    public string ManagerId { get; set; } = managerId;
    public string Country { get; set; } = country;
    public string Currency { get; set; } = currency;
    public string Team { get; set; } = team;
    public LeaveRequestAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a leave request adjustment in status open.</summary>
    public static LeaveRequestAdjustment NewLeaveRequestAdjustment(string code, string title)
    {
        return new LeaveRequestAdjustment(code, title, 0.0, "", "", "", "", "", LeaveRequestAdjustmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestAdjustmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestAdjustmentStatus next)
    {
        switch (Status)
        {
            case LeaveRequestAdjustmentStatus.Open:
                switch (next)
                {
                    case LeaveRequestAdjustmentStatus.InReview:
                        return true;
                }
                return false;
            case LeaveRequestAdjustmentStatus.InReview:
                switch (next)
                {
                    case LeaveRequestAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case LeaveRequestAdjustmentStatus.Resolved:
                return next == LeaveRequestAdjustmentStatus.Archived;
            case LeaveRequestAdjustmentStatus.Reopened:
                return next == LeaveRequestAdjustmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LeaveRequestAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public LeaveRequestAdjustment Copy()
    {
        return new LeaveRequestAdjustment(Code, Title, Amount, Grade, ManagerId, Country, Currency, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
