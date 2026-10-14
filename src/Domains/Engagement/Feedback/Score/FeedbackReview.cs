namespace Acme.Workforce.Engagement.Feedback.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback review is in its life cycle.</summary>
public enum FeedbackReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A feedback review. Its code is unique within a tenant.</summary>
public sealed class FeedbackReview(string code, string title, string location, string employeeId, string ownerId, double budget, int expiresAt, FeedbackReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public string EmployeeId { get; set; } = employeeId;
    public string OwnerId { get; set; } = ownerId;
    public double Budget { get; set; } = budget;
    public int ExpiresAt { get; set; } = expiresAt;
    public FeedbackReviewStatus Status { get; set; } = status;

    /// <summary>Starts a feedback review in status planned.</summary>
    public static FeedbackReview NewFeedbackReview(string code, string title)
    {
        return new FeedbackReview(code, title, "", "", "", 0.0, 0, FeedbackReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackReviewStatus next)
    {
        switch (Status)
        {
            case FeedbackReviewStatus.Planned:
                return next == FeedbackReviewStatus.Completed;
            case FeedbackReviewStatus.Scheduled:
                return next == FeedbackReviewStatus.InProgress || next == FeedbackReviewStatus.Cancelled;
            case FeedbackReviewStatus.InProgress:
                switch (next)
                {
                    case FeedbackReviewStatus.Cancelled:
                        return true;
                }
                return false;
            case FeedbackReviewStatus.Completed:
                return next == FeedbackReviewStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FeedbackReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"feedback review"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)ExpiresAt - Budget, 2);
    }

    public FeedbackReview Copy()
    {
        return new FeedbackReview(Code, Title, Location, EmployeeId, OwnerId, Budget, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
