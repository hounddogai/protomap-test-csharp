namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet claim is in its life cycle.</summary>
public enum TimesheetClaimStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A timesheet claim as the timesheet team keeps it.</summary>
public sealed class TimesheetClaim(string code, string title, double fte, string currency, string managerId, string phone, string grade, double rating, int updatedAt, int quantity, string skill, TimesheetClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public string Currency { get; set; } = currency;
    public string ManagerId { get; set; } = managerId;
    public string Phone { get; set; } = phone;
    public string Grade { get; set; } = grade;
    public double Rating { get; set; } = rating;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Quantity { get; set; } = quantity;
    public string Skill { get; set; } = skill;
    public TimesheetClaimStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet claim in status open.</summary>
    public static TimesheetClaim NewTimesheetClaim(string code, string title)
    {
        return new TimesheetClaim(code, title, 0.0, "", "", "", "", 0.0, 0, 0, "", TimesheetClaimStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetClaimStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetClaimStatus next)
    {
        switch (Status)
        {
            case TimesheetClaimStatus.Open:
                switch (next)
                {
                    case TimesheetClaimStatus.InReview:
                        return true;
                }
                return false;
            case TimesheetClaimStatus.InReview:
                switch (next)
                {
                    case TimesheetClaimStatus.Resolved:
                        return true;
                }
                return false;
            case TimesheetClaimStatus.Resolved:
                switch (next)
                {
                    case TimesheetClaimStatus.Archived:
                        return true;
                }
                return false;
            case TimesheetClaimStatus.Reopened:
                return next == TimesheetClaimStatus.Archived;
        }
        return false;
    }

    public void MoveTo(TimesheetClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"timesheet claim"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)UpdatedAt + Fte, 2);
    }

    public TimesheetClaim Copy()
    {
        return new TimesheetClaim(
            Code,
            Title,
            Fte,
            Currency,
            ManagerId,
            Phone,
            Grade,
            Rating,
            UpdatedAt,
            Quantity,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
