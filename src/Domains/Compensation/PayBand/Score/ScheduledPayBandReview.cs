namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pay band review is in its life cycle.</summary>
public enum ScheduledPayBandReviewStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A scheduled pay band review as the pay band team keeps it.</summary>
public sealed class ScheduledPayBandReview(string code, string title, int revision, string checksum, int windowDays, bool active, string costCenter, string description, ScheduledPayBandReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public string Checksum { get; set; } = checksum;
    public int WindowDays { get; set; } = windowDays;
    public bool Active { get; set; } = active;
    public string CostCenter { get; set; } = costCenter;
    public string Description { get; set; } = description;
    public ScheduledPayBandReviewStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pay band review in status draft.</summary>
    public static ScheduledPayBandReview NewScheduledPayBandReview(string code, string title)
    {
        return new ScheduledPayBandReview(code, title, 0, "", 0, false, "", "", ScheduledPayBandReviewStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPayBandReviewStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPayBandReviewStatus next)
    {
        switch (Status)
        {
            case ScheduledPayBandReviewStatus.Draft:
                switch (next)
                {
                    case ScheduledPayBandReviewStatus.Active:
                        return true;
                }
                return false;
            case ScheduledPayBandReviewStatus.Pending:
                return next == ScheduledPayBandReviewStatus.Closed;
            case ScheduledPayBandReviewStatus.Active:
                return next == ScheduledPayBandReviewStatus.Suspended || next == ScheduledPayBandReviewStatus.Closed;
            case ScheduledPayBandReviewStatus.Suspended:
                return next == ScheduledPayBandReviewStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ScheduledPayBandReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pay band review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"scheduled pay band review"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Revision - (double)WindowDays, 2);
    }

    public ScheduledPayBandReview Copy()
    {
        return new ScheduledPayBandReview(
            Code,
            Title,
            Revision,
            Checksum,
            WindowDays,
            Active,
            CostCenter,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
