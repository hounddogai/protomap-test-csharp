namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict visitor slot is in its life cycle.</summary>
public enum StrictVisitorSlotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A strict visitor slot as the visitor team keeps it.</summary>
public sealed class StrictVisitorSlot(string code, string name, string reviewer, string category, double rate, double rating, int capacity, string contactEmail, StrictVisitorSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public string Category { get; set; } = category;
    public double Rate { get; set; } = rate;
    public double Rating { get; set; } = rating;
    public int Capacity { get; set; } = capacity;
    public string ContactEmail { get; set; } = contactEmail;
    public StrictVisitorSlotStatus Status { get; set; } = status;

    /// <summary>Starts a strict visitor slot in status open.</summary>
    public static StrictVisitorSlot NewStrictVisitorSlot(string code, string name)
    {
        return new StrictVisitorSlot(code, name, "", "", 0.0, 0.0, 0, "", StrictVisitorSlotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictVisitorSlotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictVisitorSlotStatus next)
    {
        switch (Status)
        {
            case StrictVisitorSlotStatus.Open:
                return next == StrictVisitorSlotStatus.InReview || next == StrictVisitorSlotStatus.Reopened;
            case StrictVisitorSlotStatus.InReview:
                return next == StrictVisitorSlotStatus.Resolved || next == StrictVisitorSlotStatus.Reopened;
            case StrictVisitorSlotStatus.Resolved:
                switch (next)
                {
                    case StrictVisitorSlotStatus.Reopened:
                        return true;
                }
                return false;
            case StrictVisitorSlotStatus.Reopened:
                return next == StrictVisitorSlotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictVisitorSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict visitor slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Capacity * Rate, 2);
    }

    public StrictVisitorSlot Copy()
    {
        return new StrictVisitorSlot(Code, Name, Reviewer, Category, Rate, Rating, Capacity, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
