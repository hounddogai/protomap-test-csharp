namespace Acme.Workforce.Contractors.Rate.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate entry is in its life cycle.</summary>
public enum RateEntryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rate entry. Its code is unique within a tenant.</summary>
public sealed class RateEntry(string code, string name, int priority, double amount, string team, int durationMinutes, RateEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public double Amount { get; set; } = amount;
    public string Team { get; set; } = team;
    public int DurationMinutes { get; set; } = durationMinutes;
    public RateEntryStatus Status { get; set; } = status;

    /// <summary>Starts a rate entry in status open.</summary>
    public static RateEntry NewRateEntry(string code, string name)
    {
        return new RateEntry(code, name, 0, 0.0, "", 0, RateEntryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateEntryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateEntryStatus next)
    {
        switch (Status)
        {
            case RateEntryStatus.Open:
                switch (next)
                {
                    case RateEntryStatus.InReview:
                    case RateEntryStatus.Archived:
                        return true;
                }
                return false;
            case RateEntryStatus.InReview:
                switch (next)
                {
                    case RateEntryStatus.Resolved:
                    case RateEntryStatus.Reopened:
                        return true;
                }
                return false;
            case RateEntryStatus.Resolved:
                return next == RateEntryStatus.Reopened || next == RateEntryStatus.Archived;
            case RateEntryStatus.Reopened:
                switch (next)
                {
                    case RateEntryStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)DurationMinutes * Amount, 2);
    }

    public RateEntry Copy()
    {
        return new RateEntry(Code, Name, Priority, Amount, Team, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
