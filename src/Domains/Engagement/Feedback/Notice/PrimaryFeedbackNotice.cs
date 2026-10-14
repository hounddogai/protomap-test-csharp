namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary feedback notice is in its life cycle.</summary>
public enum PrimaryFeedbackNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One primary feedback notice, with what the planners need to act on it.</summary>
public sealed class PrimaryFeedbackNotice(string code, string title, int tenureMonths, double floorValue, int limit, PrimaryFeedbackNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public double FloorValue { get; set; } = floorValue;
    public int Limit { get; set; } = limit;
    public PrimaryFeedbackNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a primary feedback notice in status draft.</summary>
    public static PrimaryFeedbackNotice NewPrimaryFeedbackNotice(string code, string title)
    {
        return new PrimaryFeedbackNotice(code, title, 0, 0.0, 0, PrimaryFeedbackNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryFeedbackNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryFeedbackNoticeStatus next)
    {
        switch (Status)
        {
            case PrimaryFeedbackNoticeStatus.Draft:
                return next == PrimaryFeedbackNoticeStatus.Pending || next == PrimaryFeedbackNoticeStatus.Closed;
            case PrimaryFeedbackNoticeStatus.Pending:
                switch (next)
                {
                    case PrimaryFeedbackNoticeStatus.Active:
                    case PrimaryFeedbackNoticeStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryFeedbackNoticeStatus.Active:
                return next == PrimaryFeedbackNoticeStatus.Closed;
            case PrimaryFeedbackNoticeStatus.Suspended:
                switch (next)
                {
                    case PrimaryFeedbackNoticeStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryFeedbackNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary feedback notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(FloorValue - (double)TenureMonths, 2);
    }

    public PrimaryFeedbackNotice Copy()
    {
        return new PrimaryFeedbackNotice(Code, Title, TenureMonths, FloorValue, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
