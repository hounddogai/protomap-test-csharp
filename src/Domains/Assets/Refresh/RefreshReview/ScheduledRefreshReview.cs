namespace Acme.Workforce.Assets.Refresh.RefreshReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled refresh review is in its life cycle.</summary>
public enum ScheduledRefreshReviewStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One scheduled refresh review, with what the planners need to act on it.</summary>
public sealed class ScheduledRefreshReview(string code, string title, string managerId, int revision, int capacity, string summary, int tenureMonths, ScheduledRefreshReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public int Revision { get; set; } = revision;
    public int Capacity { get; set; } = capacity;
    public string Summary { get; set; } = summary;
    public int TenureMonths { get; set; } = tenureMonths;
    public ScheduledRefreshReviewStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled refresh review in status idle.</summary>
    public static ScheduledRefreshReview NewScheduledRefreshReview(string code, string title)
    {
        return new ScheduledRefreshReview(code, title, "", 0, 0, "", 0, ScheduledRefreshReviewStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledRefreshReviewStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledRefreshReviewStatus next)
    {
        switch (Status)
        {
            case ScheduledRefreshReviewStatus.Idle:
                switch (next)
                {
                    case ScheduledRefreshReviewStatus.Ready:
                        return true;
                }
                return false;
            case ScheduledRefreshReviewStatus.Warming:
                return next == ScheduledRefreshReviewStatus.Draining;
            case ScheduledRefreshReviewStatus.Ready:
                switch (next)
                {
                    case ScheduledRefreshReviewStatus.Draining:
                    case ScheduledRefreshReviewStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledRefreshReviewStatus.Draining:
                return next == ScheduledRefreshReviewStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScheduledRefreshReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled refresh review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"scheduled refresh review"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)TenureMonths - (double)Revision, 2);
    }

    public ScheduledRefreshReview Copy()
    {
        return new ScheduledRefreshReview(Code, Title, ManagerId, Revision, Capacity, Summary, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
