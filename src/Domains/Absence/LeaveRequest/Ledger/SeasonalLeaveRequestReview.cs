namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal leave request review is in its life cycle.</summary>
public enum SeasonalLeaveRequestReviewStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A seasonal leave request review. Its code is unique within a tenant.</summary>
public sealed class SeasonalLeaveRequestReview(string code, string title, string badge, double rate, int expiresAt, string skill, SeasonalLeaveRequestReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public double Rate { get; set; } = rate;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Skill { get; set; } = skill;
    public SeasonalLeaveRequestReviewStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal leave request review in status proposed.</summary>
    public static SeasonalLeaveRequestReview NewSeasonalLeaveRequestReview(string code, string title)
    {
        return new SeasonalLeaveRequestReview(
            code,
            title,
            "",
            0.0,
            0,
            "",
            SeasonalLeaveRequestReviewStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalLeaveRequestReviewStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalLeaveRequestReviewStatus next)
    {
        switch (Status)
        {
            case SeasonalLeaveRequestReviewStatus.Proposed:
                switch (next)
                {
                    case SeasonalLeaveRequestReviewStatus.Withdrawn:
                        return true;
                }
                return false;
            case SeasonalLeaveRequestReviewStatus.Approved:
                switch (next)
                {
                    case SeasonalLeaveRequestReviewStatus.Rejected:
                        return true;
                }
                return false;
            case SeasonalLeaveRequestReviewStatus.Rejected:
                return next == SeasonalLeaveRequestReviewStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SeasonalLeaveRequestReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal leave request review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)ExpiresAt - Rate, 2);
    }

    public SeasonalLeaveRequestReview Copy()
    {
        return new SeasonalLeaveRequestReview(Code, Title, Badge, Rate, ExpiresAt, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
