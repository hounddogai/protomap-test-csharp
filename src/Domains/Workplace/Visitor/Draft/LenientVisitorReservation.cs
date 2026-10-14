namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient visitor reservation is in its life cycle.</summary>
public enum LenientVisitorReservationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A lenient visitor reservation as the visitor team keeps it.</summary>
public sealed class LenientVisitorReservation(string code, string name, string location, double hours, int priority, LenientVisitorReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public double Hours { get; set; } = hours;
    public int Priority { get; set; } = priority;
    public LenientVisitorReservationStatus Status { get; set; } = status;

    /// <summary>Starts a lenient visitor reservation in status open.</summary>
    public static LenientVisitorReservation NewLenientVisitorReservation(string code, string name)
    {
        return new LenientVisitorReservation(code, name, "", 0.0, 0, LenientVisitorReservationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientVisitorReservationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientVisitorReservationStatus next)
    {
        switch (Status)
        {
            case LenientVisitorReservationStatus.Open:
                return next == LenientVisitorReservationStatus.InReview;
            case LenientVisitorReservationStatus.InReview:
                switch (next)
                {
                    case LenientVisitorReservationStatus.Reopened:
                    case LenientVisitorReservationStatus.Archived:
                        return true;
                }
                return false;
            case LenientVisitorReservationStatus.Resolved:
                switch (next)
                {
                    case LenientVisitorReservationStatus.Reopened:
                        return true;
                }
                return false;
            case LenientVisitorReservationStatus.Reopened:
                return next == LenientVisitorReservationStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LenientVisitorReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient visitor reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Priority * Hours, 2);
    }

    public LenientVisitorReservation Copy()
    {
        return new LenientVisitorReservation(Code, Name, Location, Hours, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
