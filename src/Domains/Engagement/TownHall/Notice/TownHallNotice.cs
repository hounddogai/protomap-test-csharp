namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall notice is in its life cycle.</summary>
public enum TownHallNoticeStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a town hall notice and its current status.</summary>
public sealed class TownHallNotice(string code, string name, double discount, string notes, int attempts, double rating, TownHallNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public string Notes { get; set; } = notes;
    public int Attempts { get; set; } = attempts;
    public double Rating { get; set; } = rating;
    public TownHallNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a town hall notice in status open.</summary>
    public static TownHallNotice NewTownHallNotice(string code, string name)
    {
        return new TownHallNotice(code, name, 0.0, "", 0, 0.0, TownHallNoticeStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallNoticeStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallNoticeStatus next)
    {
        switch (Status)
        {
            case TownHallNoticeStatus.Open:
                switch (next)
                {
                    case TownHallNoticeStatus.InReview:
                        return true;
                }
                return false;
            case TownHallNoticeStatus.InReview:
                switch (next)
                {
                    case TownHallNoticeStatus.Resolved:
                    case TownHallNoticeStatus.Archived:
                        return true;
                }
                return false;
            case TownHallNoticeStatus.Resolved:
                switch (next)
                {
                    case TownHallNoticeStatus.Archived:
                        return true;
                }
                return false;
            case TownHallNoticeStatus.Reopened:
                return next == TownHallNoticeStatus.Archived;
        }
        return false;
    }

    public void MoveTo(TownHallNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Discount * (double)Attempts, 2);
    }

    public TownHallNotice Copy()
    {
        return new TownHallNotice(Code, Name, Discount, Notes, Attempts, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
