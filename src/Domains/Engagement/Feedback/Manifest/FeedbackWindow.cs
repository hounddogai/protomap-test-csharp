namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback window is in its life cycle.</summary>
public enum FeedbackWindowStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A feedback window as the feedback team keeps it.</summary>
public sealed class FeedbackWindow(string code, string title, string contactEmail, int quantity, double budget, int attempts, double fte, FeedbackWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public int Quantity { get; set; } = quantity;
    public double Budget { get; set; } = budget;
    public int Attempts { get; set; } = attempts;
    public double Fte { get; set; } = fte;
    public FeedbackWindowStatus Status { get; set; } = status;

    /// <summary>Starts a feedback window in status open.</summary>
    public static FeedbackWindow NewFeedbackWindow(string code, string title)
    {
        return new FeedbackWindow(code, title, "", 0, 0.0, 0, 0.0, FeedbackWindowStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackWindowStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackWindowStatus next)
    {
        switch (Status)
        {
            case FeedbackWindowStatus.Open:
                switch (next)
                {
                    case FeedbackWindowStatus.Resolved:
                        return true;
                }
                return false;
            case FeedbackWindowStatus.InReview:
                return next == FeedbackWindowStatus.Reopened || next == FeedbackWindowStatus.Archived;
            case FeedbackWindowStatus.Resolved:
                return next == FeedbackWindowStatus.Reopened || next == FeedbackWindowStatus.Archived;
            case FeedbackWindowStatus.Reopened:
                switch (next)
                {
                    case FeedbackWindowStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"feedback window"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Fte * (double)Quantity, 2);
    }

    public FeedbackWindow Copy()
    {
        return new FeedbackWindow(Code, Title, ContactEmail, Quantity, Budget, Attempts, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
