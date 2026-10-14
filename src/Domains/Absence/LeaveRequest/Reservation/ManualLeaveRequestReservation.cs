namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual leave request reservation is in its life cycle.</summary>
public enum ManualLeaveRequestReservationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One manual leave request reservation, with what the planners need to act on it.</summary>
public sealed class ManualLeaveRequestReservation(string code, string title, string team, string label, double maximum, List<string> tags, double floorValue, double rating, string category, int startsAt, ManualLeaveRequestReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public List<string> Tags { get; set; } = tags;
    public double FloorValue { get; set; } = floorValue;
    public double Rating { get; set; } = rating;
    public string Category { get; set; } = category;
    public int StartsAt { get; set; } = startsAt;
    public ManualLeaveRequestReservationStatus Status { get; set; } = status;

    /// <summary>Starts a manual leave request reservation in status planned.</summary>
    public static ManualLeaveRequestReservation NewManualLeaveRequestReservation(string code, string title)
    {
        return new ManualLeaveRequestReservation(
            code,
            title,
            "",
            "",
            0.0,
            new List<string>(),
            0.0,
            0.0,
            "",
            0,
            ManualLeaveRequestReservationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualLeaveRequestReservationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualLeaveRequestReservationStatus next)
    {
        switch (Status)
        {
            case ManualLeaveRequestReservationStatus.Planned:
                return next == ManualLeaveRequestReservationStatus.InProgress || next == ManualLeaveRequestReservationStatus.Cancelled;
            case ManualLeaveRequestReservationStatus.Scheduled:
                switch (next)
                {
                    case ManualLeaveRequestReservationStatus.Cancelled:
                        return true;
                }
                return false;
            case ManualLeaveRequestReservationStatus.InProgress:
                switch (next)
                {
                    case ManualLeaveRequestReservationStatus.Completed:
                    case ManualLeaveRequestReservationStatus.Cancelled:
                        return true;
                }
                return false;
            case ManualLeaveRequestReservationStatus.Completed:
                switch (next)
                {
                    case ManualLeaveRequestReservationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualLeaveRequestReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual leave request reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"manual leave request reservation"} {Code}";
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

    public double ProjectedExposure()
    {
        return Math.Round(FloorValue - Maximum, 2);
    }

    public ManualLeaveRequestReservation Copy()
    {
        return new ManualLeaveRequestReservation(
            Code,
            Title,
            Team,
            Label,
            Maximum,
            Tags,
            FloorValue,
            Rating,
            Category,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
