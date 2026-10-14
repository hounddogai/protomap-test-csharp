namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request review is in its life cycle.</summary>
public enum LeaveRequestReviewStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One leave request review, with what the planners need to act on it.</summary>
public sealed class LeaveRequestReview(string code, string label, double budget, string channel, double hours, double rating, double fte, bool archived, string department, int startsAt, string category, LeaveRequestReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string Channel { get; set; } = channel;
    public double Hours { get; set; } = hours;
    public double Rating { get; set; } = rating;
    public double Fte { get; set; } = fte;
    public bool Archived { get; set; } = archived;
    public string Department { get; set; } = department;
    public int StartsAt { get; set; } = startsAt;
    public string Category { get; set; } = category;
    public LeaveRequestReviewStatus Status { get; set; } = status;

    /// <summary>Starts a leave request review in status open.</summary>
    public static LeaveRequestReview NewLeaveRequestReview(string code, string label)
    {
        return new LeaveRequestReview(
            code,
            label,
            0.0,
            "",
            0.0,
            0.0,
            0.0,
            false,
            "",
            0,
            "",
            LeaveRequestReviewStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestReviewStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestReviewStatus next)
    {
        switch (Status)
        {
            case LeaveRequestReviewStatus.Open:
                return next == LeaveRequestReviewStatus.Resolved || next == LeaveRequestReviewStatus.Archived;
            case LeaveRequestReviewStatus.InReview:
                return next == LeaveRequestReviewStatus.Reopened || next == LeaveRequestReviewStatus.Archived;
            case LeaveRequestReviewStatus.Resolved:
                return next == LeaveRequestReviewStatus.Reopened || next == LeaveRequestReviewStatus.Archived;
            case LeaveRequestReviewStatus.Reopened:
                return next == LeaveRequestReviewStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LeaveRequestReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Rating * (double)StartsAt, 2);
    }

    public LeaveRequestReview Copy()
    {
        return new LeaveRequestReview(
            Code,
            Label,
            Budget,
            Channel,
            Hours,
            Rating,
            Fte,
            Archived,
            Department,
            StartsAt,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
