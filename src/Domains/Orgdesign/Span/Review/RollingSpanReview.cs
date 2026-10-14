namespace Acme.Workforce.Orgdesign.Span.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling span review is in its life cycle.</summary>
public enum RollingSpanReviewStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rolling span review as the span team keeps it.</summary>
public sealed class RollingSpanReview(string code, string title, double threshold, string costCenter, string name, string team, int sequence, double rate, double margin, int graceDays, double amount, RollingSpanReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string CostCenter { get; set; } = costCenter;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public int Sequence { get; set; } = sequence;
    public double Rate { get; set; } = rate;
    public double Margin { get; set; } = margin;
    public int GraceDays { get; set; } = graceDays;
    public double Amount { get; set; } = amount;
    public RollingSpanReviewStatus Status { get; set; } = status;

    /// <summary>Starts a rolling span review in status open.</summary>
    public static RollingSpanReview NewRollingSpanReview(string code, string title)
    {
        return new RollingSpanReview(
            code,
            title,
            0.0,
            "",
            "",
            "",
            0,
            0.0,
            0.0,
            0,
            0.0,
            RollingSpanReviewStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingSpanReviewStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingSpanReviewStatus next)
    {
        switch (Status)
        {
            case RollingSpanReviewStatus.Open:
                return next == RollingSpanReviewStatus.InReview;
            case RollingSpanReviewStatus.InReview:
                switch (next)
                {
                    case RollingSpanReviewStatus.Reopened:
                    case RollingSpanReviewStatus.Archived:
                        return true;
                }
                return false;
            case RollingSpanReviewStatus.Resolved:
                switch (next)
                {
                    case RollingSpanReviewStatus.Reopened:
                        return true;
                }
                return false;
            case RollingSpanReviewStatus.Reopened:
                switch (next)
                {
                    case RollingSpanReviewStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingSpanReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling span review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Sequence + (double)GraceDays, 2);
    }

    public RollingSpanReview Copy()
    {
        return new RollingSpanReview(
            Code,
            Title,
            Threshold,
            CostCenter,
            Name,
            Team,
            Sequence,
            Rate,
            Margin,
            GraceDays,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
