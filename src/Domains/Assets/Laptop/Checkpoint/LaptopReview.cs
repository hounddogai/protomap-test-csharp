namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop review is in its life cycle.</summary>
public enum LaptopReviewStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One laptop review, with what the planners need to act on it.</summary>
public sealed class LaptopReview(string code, string title, bool visible, string jobCode, int durationMinutes, int version, int priority, int windowDays, string employeeId, LaptopReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public string JobCode { get; set; } = jobCode;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Version { get; set; } = version;
    public int Priority { get; set; } = priority;
    public int WindowDays { get; set; } = windowDays;
    public string EmployeeId { get; set; } = employeeId;
    public LaptopReviewStatus Status { get; set; } = status;

    /// <summary>Starts a laptop review in status draft.</summary>
    public static LaptopReview NewLaptopReview(string code, string title)
    {
        return new LaptopReview(code, title, false, "", 0, 0, 0, 0, "", LaptopReviewStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopReviewStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopReviewStatus next)
    {
        switch (Status)
        {
            case LaptopReviewStatus.Draft:
                switch (next)
                {
                    case LaptopReviewStatus.Pending:
                        return true;
                }
                return false;
            case LaptopReviewStatus.Pending:
                return next == LaptopReviewStatus.Active || next == LaptopReviewStatus.Suspended;
            case LaptopReviewStatus.Active:
                switch (next)
                {
                    case LaptopReviewStatus.Suspended:
                        return true;
                }
                return false;
            case LaptopReviewStatus.Suspended:
                return next == LaptopReviewStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LaptopReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Version - (double)Priority, 2);
    }

    public LaptopReview Copy()
    {
        return new LaptopReview(
            Code,
            Title,
            Visible,
            JobCode,
            DurationMinutes,
            Version,
            Priority,
            WindowDays,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
