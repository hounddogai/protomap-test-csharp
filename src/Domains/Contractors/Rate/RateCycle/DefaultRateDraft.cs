namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default rate draft is in its life cycle.</summary>
public enum DefaultRateDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A default rate draft. Its code is unique within a tenant.</summary>
public sealed class DefaultRateDraft(string code, string name, string summary, int durationMinutes, int capacity, DefaultRateDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Capacity { get; set; } = capacity;
    public DefaultRateDraftStatus Status { get; set; } = status;

    /// <summary>Starts a default rate draft in status open.</summary>
    public static DefaultRateDraft NewDefaultRateDraft(string code, string name)
    {
        return new DefaultRateDraft(code, name, "", 0, 0, DefaultRateDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultRateDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultRateDraftStatus next)
    {
        switch (Status)
        {
            case DefaultRateDraftStatus.Open:
                switch (next)
                {
                    case DefaultRateDraftStatus.Resolved:
                        return true;
                }
                return false;
            case DefaultRateDraftStatus.InReview:
                return next == DefaultRateDraftStatus.Resolved || next == DefaultRateDraftStatus.Archived;
            case DefaultRateDraftStatus.Resolved:
                return next == DefaultRateDraftStatus.Reopened || next == DefaultRateDraftStatus.Archived;
            case DefaultRateDraftStatus.Reopened:
                return next == DefaultRateDraftStatus.Archived;
        }
        return false;
    }

    public void MoveTo(DefaultRateDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default rate draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)DurationMinutes * (double)Capacity, 2);
    }

    public DefaultRateDraft Copy()
    {
        return new DefaultRateDraft(Code, Name, Summary, DurationMinutes, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
