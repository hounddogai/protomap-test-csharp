namespace Acme.Workforce.Talent.Nomination.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination notice is in its life cycle.</summary>
public enum NominationNoticeStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A nomination notice as the nomination team keeps it.</summary>
public sealed class NominationNotice(string code, string title, double discount, string ownerId, string location, string reviewer, string currency, int attempts, string segment, NominationNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string OwnerId { get; set; } = ownerId;
    public string Location { get; set; } = location;
    public string Reviewer { get; set; } = reviewer;
    public string Currency { get; set; } = currency;
    public int Attempts { get; set; } = attempts;
    public string Segment { get; set; } = segment;
    public NominationNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a nomination notice in status requested.</summary>
    public static NominationNotice NewNominationNotice(string code, string title)
    {
        return new NominationNotice(code, title, 0.0, "", "", "", "", 0, "", NominationNoticeStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationNoticeStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationNoticeStatus next)
    {
        switch (Status)
        {
            case NominationNoticeStatus.Requested:
                return next == NominationNoticeStatus.Granted;
            case NominationNoticeStatus.Granted:
                switch (next)
                {
                    case NominationNoticeStatus.Revoked:
                        return true;
                }
                return false;
            case NominationNoticeStatus.Revoked:
                return next == NominationNoticeStatus.Expired;
        }
        return false;
    }

    public void MoveTo(NominationNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Discount + (double)Attempts, 2);
    }

    public NominationNotice Copy()
    {
        return new NominationNotice(
            Code,
            Title,
            Discount,
            OwnerId,
            Location,
            Reviewer,
            Currency,
            Attempts,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
