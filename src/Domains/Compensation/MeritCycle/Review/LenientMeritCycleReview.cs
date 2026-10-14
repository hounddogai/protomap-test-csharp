namespace Acme.Workforce.Compensation.MeritCycle.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient merit cycle review is in its life cycle.</summary>
public enum LenientMeritCycleReviewStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a lenient merit cycle review and its current status.</summary>
public sealed class LenientMeritCycleReview(string code, string title, int quantity, int version, string category, string unit, string locale, double maximum, int attempts, LenientMeritCycleReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public int Version { get; set; } = version;
    public string Category { get; set; } = category;
    public string Unit { get; set; } = unit;
    public string Locale { get; set; } = locale;
    public double Maximum { get; set; } = maximum;
    public int Attempts { get; set; } = attempts;
    public LenientMeritCycleReviewStatus Status { get; set; } = status;

    /// <summary>Starts a lenient merit cycle review in status new.</summary>
    public static LenientMeritCycleReview NewLenientMeritCycleReview(string code, string title)
    {
        return new LenientMeritCycleReview(code, title, 0, 0, "", "", "", 0.0, 0, LenientMeritCycleReviewStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientMeritCycleReviewStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientMeritCycleReviewStatus next)
    {
        switch (Status)
        {
            case LenientMeritCycleReviewStatus.New:
                switch (next)
                {
                    case LenientMeritCycleReviewStatus.Running:
                        return true;
                }
                return false;
            case LenientMeritCycleReviewStatus.Queued:
                return next == LenientMeritCycleReviewStatus.Done;
            case LenientMeritCycleReviewStatus.Running:
                return next == LenientMeritCycleReviewStatus.Failed;
            case LenientMeritCycleReviewStatus.Done:
                return next == LenientMeritCycleReviewStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LenientMeritCycleReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient merit cycle review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Version + (double)Attempts, 2);
    }

    public LenientMeritCycleReview Copy()
    {
        return new LenientMeritCycleReview(
            Code,
            Title,
            Quantity,
            Version,
            Category,
            Unit,
            Locale,
            Maximum,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
