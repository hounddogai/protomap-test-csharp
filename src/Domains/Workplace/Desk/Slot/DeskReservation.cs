namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk reservation is in its life cycle.</summary>
public enum DeskReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One desk reservation, with what the planners need to act on it.</summary>
public sealed class DeskReservation(string code, string label, string country, int attempts, List<string> tags, DeskReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public int Attempts { get; set; } = attempts;
    public List<string> Tags { get; set; } = tags;
    public DeskReservationStatus Status { get; set; } = status;

    /// <summary>Starts a desk reservation in status proposed.</summary>
    public static DeskReservation NewDeskReservation(string code, string label)
    {
        return new DeskReservation(code, label, "", 0, new List<string>(), DeskReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskReservationStatus next)
    {
        switch (Status)
        {
            case DeskReservationStatus.Proposed:
                return next == DeskReservationStatus.Approved || next == DeskReservationStatus.Withdrawn;
            case DeskReservationStatus.Approved:
                switch (next)
                {
                    case DeskReservationStatus.Rejected:
                        return true;
                }
                return false;
            case DeskReservationStatus.Rejected:
                switch (next)
                {
                    case DeskReservationStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public DeskReservation Copy()
    {
        return new DeskReservation(Code, Label, Country, Attempts, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
