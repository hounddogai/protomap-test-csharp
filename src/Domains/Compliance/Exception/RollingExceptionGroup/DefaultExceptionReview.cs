namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default exception review is in its life cycle.</summary>
public enum DefaultExceptionReviewStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A default exception review as the exception team keeps it.</summary>
public sealed class DefaultExceptionReview(string code, string label, string team, string description, int createdAt, string name, int capacity, int windowDays, DefaultExceptionReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public string Description { get; set; } = description;
    public int CreatedAt { get; set; } = createdAt;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public int WindowDays { get; set; } = windowDays;
    public DefaultExceptionReviewStatus Status { get; set; } = status;

    /// <summary>Starts a default exception review in status new.</summary>
    public static DefaultExceptionReview NewDefaultExceptionReview(string code, string label)
    {
        return new DefaultExceptionReview(code, label, "", "", 0, "", 0, 0, DefaultExceptionReviewStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultExceptionReviewStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultExceptionReviewStatus next)
    {
        switch (Status)
        {
            case DefaultExceptionReviewStatus.New:
                switch (next)
                {
                    case DefaultExceptionReviewStatus.Done:
                        return true;
                }
                return false;
            case DefaultExceptionReviewStatus.Queued:
                return next == DefaultExceptionReviewStatus.Running;
            case DefaultExceptionReviewStatus.Running:
                return next == DefaultExceptionReviewStatus.Done || next == DefaultExceptionReviewStatus.Failed;
            case DefaultExceptionReviewStatus.Done:
                return next == DefaultExceptionReviewStatus.Failed;
        }
        return false;
    }

    public void MoveTo(DefaultExceptionReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default exception review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)CreatedAt + (double)WindowDays, 2);
    }

    public DefaultExceptionReview Copy()
    {
        return new DefaultExceptionReview(
            Code,
            Label,
            Team,
            Description,
            CreatedAt,
            Name,
            Capacity,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
