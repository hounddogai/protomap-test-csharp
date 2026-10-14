namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan review is in its life cycle.</summary>
public enum SuccessionPlanReviewStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a succession plan review and its current status.</summary>
public sealed class SuccessionPlanReview(string code, string title, bool approved, int batchSize, double budget, int priority, SuccessionPlanReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public int BatchSize { get; set; } = batchSize;
    public double Budget { get; set; } = budget;
    public int Priority { get; set; } = priority;
    public SuccessionPlanReviewStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan review in status requested.</summary>
    public static SuccessionPlanReview NewSuccessionPlanReview(string code, string title)
    {
        return new SuccessionPlanReview(code, title, false, 0, 0.0, 0, SuccessionPlanReviewStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanReviewStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanReviewStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanReviewStatus.Requested:
                return next == SuccessionPlanReviewStatus.Granted || next == SuccessionPlanReviewStatus.Expired;
            case SuccessionPlanReviewStatus.Granted:
                switch (next)
                {
                    case SuccessionPlanReviewStatus.Revoked:
                    case SuccessionPlanReviewStatus.Expired:
                        return true;
                }
                return false;
            case SuccessionPlanReviewStatus.Revoked:
                return next == SuccessionPlanReviewStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Priority - (double)BatchSize, 2);
    }

    public SuccessionPlanReview Copy()
    {
        return new SuccessionPlanReview(Code, Title, Approved, BatchSize, Budget, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
