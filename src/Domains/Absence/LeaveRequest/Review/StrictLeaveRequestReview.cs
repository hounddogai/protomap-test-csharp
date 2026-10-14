namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict leave request review is in its life cycle.</summary>
public enum StrictLeaveRequestReviewStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a strict leave request review and its current status.</summary>
public sealed class StrictLeaveRequestReview(string code, string title, string category, string grade, string costCenter, string name, double amount, StrictLeaveRequestReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string Grade { get; set; } = grade;
    public string CostCenter { get; set; } = costCenter;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public StrictLeaveRequestReviewStatus Status { get; set; } = status;

    /// <summary>Starts a strict leave request review in status idle.</summary>
    public static StrictLeaveRequestReview NewStrictLeaveRequestReview(string code, string title)
    {
        return new StrictLeaveRequestReview(code, title, "", "", "", "", 0.0, StrictLeaveRequestReviewStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictLeaveRequestReviewStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictLeaveRequestReviewStatus next)
    {
        switch (Status)
        {
            case StrictLeaveRequestReviewStatus.Idle:
                switch (next)
                {
                    case StrictLeaveRequestReviewStatus.Draining:
                        return true;
                }
                return false;
            case StrictLeaveRequestReviewStatus.Warming:
                return next == StrictLeaveRequestReviewStatus.Ready || next == StrictLeaveRequestReviewStatus.Stopped;
            case StrictLeaveRequestReviewStatus.Ready:
                return next == StrictLeaveRequestReviewStatus.Stopped;
            case StrictLeaveRequestReviewStatus.Draining:
                return next == StrictLeaveRequestReviewStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StrictLeaveRequestReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict leave request review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public StrictLeaveRequestReview Copy()
    {
        return new StrictLeaveRequestReview(Code, Title, Category, Grade, CostCenter, Name, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
