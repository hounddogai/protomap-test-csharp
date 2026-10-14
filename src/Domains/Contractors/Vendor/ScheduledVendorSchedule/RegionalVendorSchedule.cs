namespace Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional vendor schedule is in its life cycle.</summary>
public enum RegionalVendorScheduleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A regional vendor schedule as the vendor team keeps it.</summary>
public sealed class RegionalVendorSchedule(string code, string title, bool archived, int attempts, double threshold, double cost, int endsAt, RegionalVendorScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public int Attempts { get; set; } = attempts;
    public double Threshold { get; set; } = threshold;
    public double Cost { get; set; } = cost;
    public int EndsAt { get; set; } = endsAt;
    public RegionalVendorScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a regional vendor schedule in status idle.</summary>
    public static RegionalVendorSchedule NewRegionalVendorSchedule(string code, string title)
    {
        return new RegionalVendorSchedule(code, title, false, 0, 0.0, 0.0, 0, RegionalVendorScheduleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalVendorScheduleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalVendorScheduleStatus next)
    {
        switch (Status)
        {
            case RegionalVendorScheduleStatus.Idle:
                switch (next)
                {
                    case RegionalVendorScheduleStatus.Ready:
                    case RegionalVendorScheduleStatus.Draining:
                        return true;
                }
                return false;
            case RegionalVendorScheduleStatus.Warming:
                return next == RegionalVendorScheduleStatus.Ready || next == RegionalVendorScheduleStatus.Draining;
            case RegionalVendorScheduleStatus.Ready:
                switch (next)
                {
                    case RegionalVendorScheduleStatus.Draining:
                        return true;
                }
                return false;
            case RegionalVendorScheduleStatus.Draining:
                return next == RegionalVendorScheduleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RegionalVendorScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional vendor schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetVolume()
    {
        return Math.Round((double)EndsAt + Threshold, 2);
    }

    public RegionalVendorSchedule Copy()
    {
        return new RegionalVendorSchedule(Code, Title, Archived, Attempts, Threshold, Cost, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
