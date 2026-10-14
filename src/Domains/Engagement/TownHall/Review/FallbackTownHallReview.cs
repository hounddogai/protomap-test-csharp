namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback town hall review is in its life cycle.</summary>
public enum FallbackTownHallReviewStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A fallback town hall review. Its code is unique within a tenant.</summary>
public sealed class FallbackTownHallReview(string code, string label, string employeeId, string externalRef, string name, double score, FallbackTownHallReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public string ExternalRef { get; set; } = externalRef;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public FallbackTownHallReviewStatus Status { get; set; } = status;

    /// <summary>Starts a fallback town hall review in status new.</summary>
    public static FallbackTownHallReview NewFallbackTownHallReview(string code, string label)
    {
        return new FallbackTownHallReview(code, label, "", "", "", 0.0, FallbackTownHallReviewStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackTownHallReviewStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackTownHallReviewStatus next)
    {
        switch (Status)
        {
            case FallbackTownHallReviewStatus.New:
                return next == FallbackTownHallReviewStatus.Queued || next == FallbackTownHallReviewStatus.Done;
            case FallbackTownHallReviewStatus.Queued:
                return next == FallbackTownHallReviewStatus.Failed;
            case FallbackTownHallReviewStatus.Running:
                switch (next)
                {
                    case FallbackTownHallReviewStatus.Done:
                        return true;
                }
                return false;
            case FallbackTownHallReviewStatus.Done:
                switch (next)
                {
                    case FallbackTownHallReviewStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackTownHallReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback town hall review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback town hall review"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public FallbackTownHallReview Copy()
    {
        return new FallbackTownHallReview(Code, Label, EmployeeId, ExternalRef, Name, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
