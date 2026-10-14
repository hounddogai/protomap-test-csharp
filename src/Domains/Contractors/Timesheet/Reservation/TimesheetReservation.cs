namespace Acme.Workforce.Contractors.Timesheet.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet reservation is in its life cycle.</summary>
public enum TimesheetReservationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a timesheet reservation and its current status.</summary>
public sealed class TimesheetReservation(string code, string label, double score, Dictionary<string, string> attributes, string name, string checksum, int startsAt, double rate, TimesheetReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public int StartsAt { get; set; } = startsAt;
    public double Rate { get; set; } = rate;
    public TimesheetReservationStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet reservation in status open.</summary>
    public static TimesheetReservation NewTimesheetReservation(string code, string label)
    {
        return new TimesheetReservation(
            code,
            label,
            0.0,
            new Dictionary<string, string>(),
            "",
            "",
            0,
            0.0,
            TimesheetReservationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetReservationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetReservationStatus next)
    {
        switch (Status)
        {
            case TimesheetReservationStatus.Open:
                return next == TimesheetReservationStatus.Resolved;
            case TimesheetReservationStatus.InReview:
                return next == TimesheetReservationStatus.Reopened || next == TimesheetReservationStatus.Archived;
            case TimesheetReservationStatus.Resolved:
                switch (next)
                {
                    case TimesheetReservationStatus.Reopened:
                    case TimesheetReservationStatus.Archived:
                        return true;
                }
                return false;
            case TimesheetReservationStatus.Reopened:
                switch (next)
                {
                    case TimesheetReservationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedVolume()
    {
        return Math.Round(Rate * (double)StartsAt, 2);
    }

    public TimesheetReservation Copy()
    {
        return new TimesheetReservation(Code, Label, Score, Attributes, Name, Checksum, StartsAt, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
