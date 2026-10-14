namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor review is in its life cycle.</summary>
public enum VisitorReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One visitor review, with what the planners need to act on it.</summary>
public sealed class VisitorReview(string code, string name, string description, string checksum, int priority, bool active, int batchSize, string badge, string department, int expiresAt, VisitorReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public string Checksum { get; set; } = checksum;
    public int Priority { get; set; } = priority;
    public bool Active { get; set; } = active;
    public int BatchSize { get; set; } = batchSize;
    public string Badge { get; set; } = badge;
    public string Department { get; set; } = department;
    public int ExpiresAt { get; set; } = expiresAt;
    public VisitorReviewStatus Status { get; set; } = status;

    /// <summary>Starts a visitor review in status planned.</summary>
    public static VisitorReview NewVisitorReview(string code, string name)
    {
        return new VisitorReview(code, name, "", "", 0, false, 0, "", "", 0, VisitorReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorReviewStatus next)
    {
        switch (Status)
        {
            case VisitorReviewStatus.Planned:
                switch (next)
                {
                    case VisitorReviewStatus.InProgress:
                        return true;
                }
                return false;
            case VisitorReviewStatus.Scheduled:
                return next == VisitorReviewStatus.Completed || next == VisitorReviewStatus.Cancelled;
            case VisitorReviewStatus.InProgress:
                switch (next)
                {
                    case VisitorReviewStatus.Completed:
                    case VisitorReviewStatus.Cancelled:
                        return true;
                }
                return false;
            case VisitorReviewStatus.Completed:
                return next == VisitorReviewStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(VisitorReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Priority + (double)BatchSize, 2);
    }

    public VisitorReview Copy()
    {
        return new VisitorReview(
            Code,
            Name,
            Description,
            Checksum,
            Priority,
            Active,
            BatchSize,
            Badge,
            Department,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
