namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual override is in its life cycle.</summary>
public enum AccrualOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One accrual override, with what the planners need to act on it.</summary>
public sealed class AccrualOverride(string code, string title, string jobCode, int windowDays, string description, double weight, bool active, bool visible, int batchSize, AccrualOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public int WindowDays { get; set; } = windowDays;
    public string Description { get; set; } = description;
    public double Weight { get; set; } = weight;
    public bool Active { get; set; } = active;
    public bool Visible { get; set; } = visible;
    public int BatchSize { get; set; } = batchSize;
    public AccrualOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a accrual override in status open.</summary>
    public static AccrualOverride NewAccrualOverride(string code, string title)
    {
        return new AccrualOverride(code, title, "", 0, "", 0.0, false, false, 0, AccrualOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualOverrideStatus next)
    {
        switch (Status)
        {
            case AccrualOverrideStatus.Open:
                switch (next)
                {
                    case AccrualOverrideStatus.Resolved:
                        return true;
                }
                return false;
            case AccrualOverrideStatus.InReview:
                return next == AccrualOverrideStatus.Resolved || next == AccrualOverrideStatus.Archived;
            case AccrualOverrideStatus.Resolved:
                return next == AccrualOverrideStatus.Archived;
            case AccrualOverrideStatus.Reopened:
                return next == AccrualOverrideStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AccrualOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)WindowDays - (double)BatchSize, 2);
    }

    public AccrualOverride Copy()
    {
        return new AccrualOverride(
            Code,
            Title,
            JobCode,
            WindowDays,
            Description,
            Weight,
            Active,
            Visible,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
