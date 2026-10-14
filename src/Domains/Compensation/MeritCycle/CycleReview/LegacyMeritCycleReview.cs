namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy merit cycle review is in its life cycle.</summary>
public enum LegacyMeritCycleReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a legacy merit cycle review and its current status.</summary>
public sealed class LegacyMeritCycleReview(string code, string name, bool pinned, string ownerId, int leaveDays, LegacyMeritCycleReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public string OwnerId { get; set; } = ownerId;
    public int LeaveDays { get; set; } = leaveDays;
    public LegacyMeritCycleReviewStatus Status { get; set; } = status;

    /// <summary>Starts a legacy merit cycle review in status planned.</summary>
    public static LegacyMeritCycleReview NewLegacyMeritCycleReview(string code, string name)
    {
        return new LegacyMeritCycleReview(code, name, false, "", 0, LegacyMeritCycleReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyMeritCycleReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyMeritCycleReviewStatus next)
    {
        switch (Status)
        {
            case LegacyMeritCycleReviewStatus.Planned:
                return next == LegacyMeritCycleReviewStatus.Scheduled || next == LegacyMeritCycleReviewStatus.Completed;
            case LegacyMeritCycleReviewStatus.Scheduled:
                switch (next)
                {
                    case LegacyMeritCycleReviewStatus.InProgress:
                    case LegacyMeritCycleReviewStatus.Cancelled:
                        return true;
                }
                return false;
            case LegacyMeritCycleReviewStatus.InProgress:
                return next == LegacyMeritCycleReviewStatus.Completed;
            case LegacyMeritCycleReviewStatus.Completed:
                return next == LegacyMeritCycleReviewStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LegacyMeritCycleReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy merit cycle review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public LegacyMeritCycleReview Copy()
    {
        return new LegacyMeritCycleReview(Code, Name, Pinned, OwnerId, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
