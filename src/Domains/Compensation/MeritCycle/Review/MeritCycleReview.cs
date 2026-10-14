namespace Acme.Workforce.Compensation.MeritCycle.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle review is in its life cycle.</summary>
public enum MeritCycleReviewStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A merit cycle review. Its code is unique within a tenant.</summary>
public sealed class MeritCycleReview(string code, string title, bool pinned, double ratio, int updatedAt, string label, bool approved, MeritCycleReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public double Ratio { get; set; } = ratio;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public MeritCycleReviewStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle review in status proposed.</summary>
    public static MeritCycleReview NewMeritCycleReview(string code, string title)
    {
        return new MeritCycleReview(code, title, false, 0.0, 0, "", false, MeritCycleReviewStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleReviewStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleReviewStatus next)
    {
        switch (Status)
        {
            case MeritCycleReviewStatus.Proposed:
                return next == MeritCycleReviewStatus.Withdrawn;
            case MeritCycleReviewStatus.Approved:
                switch (next)
                {
                    case MeritCycleReviewStatus.Rejected:
                        return true;
                }
                return false;
            case MeritCycleReviewStatus.Rejected:
                switch (next)
                {
                    case MeritCycleReviewStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Ratio * (double)UpdatedAt, 2);
    }

    public MeritCycleReview Copy()
    {
        return new MeritCycleReview(Code, Title, Pinned, Ratio, UpdatedAt, Label, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
