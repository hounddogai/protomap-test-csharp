namespace Acme.Workforce.Absence.Accrual.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary accrual notice is in its life cycle.</summary>
public enum PrimaryAccrualNoticeStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a primary accrual notice and its current status.</summary>
public sealed class PrimaryAccrualNotice(string code, string name, bool archived, string country, int startsAt, PrimaryAccrualNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string Country { get; set; } = country;
    public int StartsAt { get; set; } = startsAt;
    public PrimaryAccrualNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a primary accrual notice in status open.</summary>
    public static PrimaryAccrualNotice NewPrimaryAccrualNotice(string code, string name)
    {
        return new PrimaryAccrualNotice(code, name, false, "", 0, PrimaryAccrualNoticeStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAccrualNoticeStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAccrualNoticeStatus next)
    {
        switch (Status)
        {
            case PrimaryAccrualNoticeStatus.Open:
                switch (next)
                {
                    case PrimaryAccrualNoticeStatus.Reopened:
                        return true;
                }
                return false;
            case PrimaryAccrualNoticeStatus.InReview:
                return next == PrimaryAccrualNoticeStatus.Resolved || next == PrimaryAccrualNoticeStatus.Archived;
            case PrimaryAccrualNoticeStatus.Resolved:
                switch (next)
                {
                    case PrimaryAccrualNoticeStatus.Reopened:
                    case PrimaryAccrualNoticeStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryAccrualNoticeStatus.Reopened:
                switch (next)
                {
                    case PrimaryAccrualNoticeStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryAccrualNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary accrual notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"primary accrual notice"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public PrimaryAccrualNotice Copy()
    {
        return new PrimaryAccrualNotice(Code, Name, Archived, Country, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
