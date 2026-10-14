namespace Acme.Workforce.Assets.Assignment.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment review is in its life cycle.</summary>
public enum AssignmentReviewStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One assignment review, with what the planners need to act on it.</summary>
public sealed class AssignmentReview(string code, string title, string contactEmail, int graceDays, double floorValue, string team, AssignmentReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public int GraceDays { get; set; } = graceDays;
    public double FloorValue { get; set; } = floorValue;
    public string Team { get; set; } = team;
    public AssignmentReviewStatus Status { get; set; } = status;

    /// <summary>Starts a assignment review in status new.</summary>
    public static AssignmentReview NewAssignmentReview(string code, string title)
    {
        return new AssignmentReview(code, title, "", 0, 0.0, "", AssignmentReviewStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentReviewStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentReviewStatus next)
    {
        switch (Status)
        {
            case AssignmentReviewStatus.New:
                return next == AssignmentReviewStatus.Failed;
            case AssignmentReviewStatus.Queued:
                switch (next)
                {
                    case AssignmentReviewStatus.Running:
                        return true;
                }
                return false;
            case AssignmentReviewStatus.Running:
                return next == AssignmentReviewStatus.Done || next == AssignmentReviewStatus.Failed;
            case AssignmentReviewStatus.Done:
                return next == AssignmentReviewStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AssignmentReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)GraceDays * FloorValue, 2);
    }

    public AssignmentReview Copy()
    {
        return new AssignmentReview(Code, Title, ContactEmail, GraceDays, FloorValue, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
