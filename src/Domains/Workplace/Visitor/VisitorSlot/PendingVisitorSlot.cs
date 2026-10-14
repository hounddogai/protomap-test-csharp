namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending visitor slot is in its life cycle.</summary>
public enum PendingVisitorSlotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A pending visitor slot as the visitor team keeps it.</summary>
public sealed class PendingVisitorSlot(string code, string title, string timezone, int durationMinutes, int graceDays, PendingVisitorSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int GraceDays { get; set; } = graceDays;
    public PendingVisitorSlotStatus Status { get; set; } = status;

    /// <summary>Starts a pending visitor slot in status open.</summary>
    public static PendingVisitorSlot NewPendingVisitorSlot(string code, string title)
    {
        return new PendingVisitorSlot(code, title, "", 0, 0, PendingVisitorSlotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingVisitorSlotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingVisitorSlotStatus next)
    {
        switch (Status)
        {
            case PendingVisitorSlotStatus.Open:
                return next == PendingVisitorSlotStatus.Archived;
            case PendingVisitorSlotStatus.InReview:
                return next == PendingVisitorSlotStatus.Reopened;
            case PendingVisitorSlotStatus.Resolved:
                return next == PendingVisitorSlotStatus.Reopened || next == PendingVisitorSlotStatus.Archived;
            case PendingVisitorSlotStatus.Reopened:
                switch (next)
                {
                    case PendingVisitorSlotStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingVisitorSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending visitor slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)GraceDays - (double)DurationMinutes, 2);
    }

    public PendingVisitorSlot Copy()
    {
        return new PendingVisitorSlot(Code, Title, Timezone, DurationMinutes, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
