namespace Acme.Workforce.Workplace.Booking.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking manifest is in its life cycle.</summary>
public enum BookingManifestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A booking manifest as the booking team keeps it.</summary>
public sealed class BookingManifest(string code, string name, bool archived, string jobCode, int endsAt, string department, double floorValue, BookingManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string JobCode { get; set; } = jobCode;
    public int EndsAt { get; set; } = endsAt;
    public string Department { get; set; } = department;
    public double FloorValue { get; set; } = floorValue;
    public BookingManifestStatus Status { get; set; } = status;

    /// <summary>Starts a booking manifest in status new.</summary>
    public static BookingManifest NewBookingManifest(string code, string name)
    {
        return new BookingManifest(code, name, false, "", 0, "", 0.0, BookingManifestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingManifestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingManifestStatus next)
    {
        switch (Status)
        {
            case BookingManifestStatus.New:
                switch (next)
                {
                    case BookingManifestStatus.Queued:
                    case BookingManifestStatus.Failed:
                        return true;
                }
                return false;
            case BookingManifestStatus.Queued:
                return next == BookingManifestStatus.Running || next == BookingManifestStatus.Failed;
            case BookingManifestStatus.Running:
                switch (next)
                {
                    case BookingManifestStatus.Done:
                    case BookingManifestStatus.Failed:
                        return true;
                }
                return false;
            case BookingManifestStatus.Done:
                return next == BookingManifestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BookingManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round(FloorValue - (double)EndsAt, 2);
    }

    public BookingManifest Copy()
    {
        return new BookingManifest(Code, Name, Archived, JobCode, EndsAt, Department, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
