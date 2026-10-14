namespace Acme.Workforce.Absence.Holiday.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient holiday window is in its life cycle.</summary>
public enum LenientHolidayWindowStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a lenient holiday window and its current status.</summary>
public sealed class LenientHolidayWindow(string code, string name, string notes, string source, int revision, string title, string managerId, LenientHolidayWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public string Source { get; set; } = source;
    public int Revision { get; set; } = revision;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public LenientHolidayWindowStatus Status { get; set; } = status;

    /// <summary>Starts a lenient holiday window in status draft.</summary>
    public static LenientHolidayWindow NewLenientHolidayWindow(string code, string name)
    {
        return new LenientHolidayWindow(code, name, "", "", 0, "", "", LenientHolidayWindowStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientHolidayWindowStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientHolidayWindowStatus next)
    {
        switch (Status)
        {
            case LenientHolidayWindowStatus.Draft:
                return next == LenientHolidayWindowStatus.Active;
            case LenientHolidayWindowStatus.Pending:
                return next == LenientHolidayWindowStatus.Active || next == LenientHolidayWindowStatus.Suspended;
            case LenientHolidayWindowStatus.Active:
                return next == LenientHolidayWindowStatus.Suspended || next == LenientHolidayWindowStatus.Closed;
            case LenientHolidayWindowStatus.Suspended:
                switch (next)
                {
                    case LenientHolidayWindowStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientHolidayWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient holiday window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"lenient holiday window"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public LenientHolidayWindow Copy()
    {
        return new LenientHolidayWindow(Code, Name, Notes, Source, Revision, Title, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
