namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual booking assignment is in its life cycle.</summary>
public enum ManualBookingAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A manual booking assignment as the booking team keeps it.</summary>
public sealed class ManualBookingAssignment(string code, string name, double weight, int priority, string checksum, int version, string region, ManualBookingAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public int Priority { get; set; } = priority;
    public string Checksum { get; set; } = checksum;
    public int Version { get; set; } = version;
    public string Region { get; set; } = region;
    public ManualBookingAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a manual booking assignment in status open.</summary>
    public static ManualBookingAssignment NewManualBookingAssignment(string code, string name)
    {
        return new ManualBookingAssignment(code, name, 0.0, 0, "", 0, "", ManualBookingAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBookingAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBookingAssignmentStatus next)
    {
        switch (Status)
        {
            case ManualBookingAssignmentStatus.Open:
                switch (next)
                {
                    case ManualBookingAssignmentStatus.Reopened:
                        return true;
                }
                return false;
            case ManualBookingAssignmentStatus.InReview:
                return next == ManualBookingAssignmentStatus.Resolved || next == ManualBookingAssignmentStatus.Reopened;
            case ManualBookingAssignmentStatus.Resolved:
                switch (next)
                {
                    case ManualBookingAssignmentStatus.Archived:
                        return true;
                }
                return false;
            case ManualBookingAssignmentStatus.Reopened:
                switch (next)
                {
                    case ManualBookingAssignmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualBookingAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual booking assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Version + Weight, 2);
    }

    public ManualBookingAssignment Copy()
    {
        return new ManualBookingAssignment(Code, Name, Weight, Priority, Checksum, Version, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
