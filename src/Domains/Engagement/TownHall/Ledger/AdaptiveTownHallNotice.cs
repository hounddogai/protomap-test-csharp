namespace Acme.Workforce.Engagement.TownHall.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive town hall notice is in its life cycle.</summary>
public enum AdaptiveTownHallNoticeStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a adaptive town hall notice and its current status.</summary>
public sealed class AdaptiveTownHallNotice(string code, string title, int updatedAt, string costCenter, string segment, double score, double budget, string country, string locale, bool archived, double cost, AdaptiveTownHallNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public string CostCenter { get; set; } = costCenter;
    public string Segment { get; set; } = segment;
    public double Score { get; set; } = score;
    public double Budget { get; set; } = budget;
    public string Country { get; set; } = country;
    public string Locale { get; set; } = locale;
    public bool Archived { get; set; } = archived;
    public double Cost { get; set; } = cost;
    public AdaptiveTownHallNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive town hall notice in status open.</summary>
    public static AdaptiveTownHallNotice NewAdaptiveTownHallNotice(string code, string title)
    {
        return new AdaptiveTownHallNotice(
            code,
            title,
            0,
            "",
            "",
            0.0,
            0.0,
            "",
            "",
            false,
            0.0,
            AdaptiveTownHallNoticeStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTownHallNoticeStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTownHallNoticeStatus next)
    {
        switch (Status)
        {
            case AdaptiveTownHallNoticeStatus.Open:
                switch (next)
                {
                    case AdaptiveTownHallNoticeStatus.Reopened:
                    case AdaptiveTownHallNoticeStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveTownHallNoticeStatus.InReview:
                return next == AdaptiveTownHallNoticeStatus.Reopened || next == AdaptiveTownHallNoticeStatus.Archived;
            case AdaptiveTownHallNoticeStatus.Resolved:
                switch (next)
                {
                    case AdaptiveTownHallNoticeStatus.Reopened:
                    case AdaptiveTownHallNoticeStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveTownHallNoticeStatus.Reopened:
                return next == AdaptiveTownHallNoticeStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptiveTownHallNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive town hall notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Cost - Score, 2);
    }

    public AdaptiveTownHallNotice Copy()
    {
        return new AdaptiveTownHallNotice(
            Code,
            Title,
            UpdatedAt,
            CostCenter,
            Segment,
            Score,
            Budget,
            Country,
            Locale,
            Archived,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
