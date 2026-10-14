namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment entry is in its life cycle.</summary>
public enum GarnishmentEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A garnishment entry as the garnishment team keeps it.</summary>
public sealed class GarnishmentEntry(string code, string title, double fte, int limit, int durationMinutes, GarnishmentEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public int Limit { get; set; } = limit;
    public int DurationMinutes { get; set; } = durationMinutes;
    public GarnishmentEntryStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment entry in status proposed.</summary>
    public static GarnishmentEntry NewGarnishmentEntry(string code, string title)
    {
        return new GarnishmentEntry(code, title, 0.0, 0, 0, GarnishmentEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentEntryStatus next)
    {
        switch (Status)
        {
            case GarnishmentEntryStatus.Proposed:
                return next == GarnishmentEntryStatus.Approved || next == GarnishmentEntryStatus.Withdrawn;
            case GarnishmentEntryStatus.Approved:
                switch (next)
                {
                    case GarnishmentEntryStatus.Withdrawn:
                        return true;
                }
                return false;
            case GarnishmentEntryStatus.Rejected:
                return next == GarnishmentEntryStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(GarnishmentEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Limit + Fte, 2);
    }

    public GarnishmentEntry Copy()
    {
        return new GarnishmentEntry(Code, Title, Fte, Limit, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
