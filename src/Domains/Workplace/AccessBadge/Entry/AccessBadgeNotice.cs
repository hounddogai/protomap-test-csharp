namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge notice is in its life cycle.</summary>
public enum AccessBadgeNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a access badge notice and its current status.</summary>
public sealed class AccessBadgeNotice(string code, string title, string currency, double fte, bool approved, string skill, AccessBadgeNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public double Fte { get; set; } = fte;
    public bool Approved { get; set; } = approved;
    public string Skill { get; set; } = skill;
    public AccessBadgeNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a access badge notice in status draft.</summary>
    public static AccessBadgeNotice NewAccessBadgeNotice(string code, string title)
    {
        return new AccessBadgeNotice(code, title, "", 0.0, false, "", AccessBadgeNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeNoticeStatus next)
    {
        switch (Status)
        {
            case AccessBadgeNoticeStatus.Draft:
                switch (next)
                {
                    case AccessBadgeNoticeStatus.Suspended:
                        return true;
                }
                return false;
            case AccessBadgeNoticeStatus.Pending:
                return next == AccessBadgeNoticeStatus.Active;
            case AccessBadgeNoticeStatus.Active:
                return next == AccessBadgeNoticeStatus.Suspended;
            case AccessBadgeNoticeStatus.Suspended:
                switch (next)
                {
                    case AccessBadgeNoticeStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public AccessBadgeNotice Copy()
    {
        return new AccessBadgeNotice(Code, Title, Currency, Fte, Approved, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
