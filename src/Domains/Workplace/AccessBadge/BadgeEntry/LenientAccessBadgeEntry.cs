namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient access badge entry is in its life cycle.</summary>
public enum LenientAccessBadgeEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One lenient access badge entry, with what the planners need to act on it.</summary>
public sealed class LenientAccessBadgeEntry(string code, string title, double margin, bool approved, int revision, string employeeId, int quantity, string phone, double cost, LenientAccessBadgeEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public bool Approved { get; set; } = approved;
    public int Revision { get; set; } = revision;
    public string EmployeeId { get; set; } = employeeId;
    public int Quantity { get; set; } = quantity;
    public string Phone { get; set; } = phone;
    public double Cost { get; set; } = cost;
    public LenientAccessBadgeEntryStatus Status { get; set; } = status;

    /// <summary>Starts a lenient access badge entry in status new.</summary>
    public static LenientAccessBadgeEntry NewLenientAccessBadgeEntry(string code, string title)
    {
        return new LenientAccessBadgeEntry(
            code,
            title,
            0.0,
            false,
            0,
            "",
            0,
            "",
            0.0,
            LenientAccessBadgeEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientAccessBadgeEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientAccessBadgeEntryStatus next)
    {
        switch (Status)
        {
            case LenientAccessBadgeEntryStatus.New:
                switch (next)
                {
                    case LenientAccessBadgeEntryStatus.Queued:
                    case LenientAccessBadgeEntryStatus.Failed:
                        return true;
                }
                return false;
            case LenientAccessBadgeEntryStatus.Queued:
                return next == LenientAccessBadgeEntryStatus.Done;
            case LenientAccessBadgeEntryStatus.Running:
                return next == LenientAccessBadgeEntryStatus.Done || next == LenientAccessBadgeEntryStatus.Failed;
            case LenientAccessBadgeEntryStatus.Done:
                switch (next)
                {
                    case LenientAccessBadgeEntryStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientAccessBadgeEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient access badge entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"lenient access badge entry"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Margin + (double)Quantity, 2);
    }

    public LenientAccessBadgeEntry Copy()
    {
        return new LenientAccessBadgeEntry(
            Code,
            Title,
            Margin,
            Approved,
            Revision,
            EmployeeId,
            Quantity,
            Phone,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
