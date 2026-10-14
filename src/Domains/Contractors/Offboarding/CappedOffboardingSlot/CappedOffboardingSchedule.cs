namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped offboarding schedule is in its life cycle.</summary>
public enum CappedOffboardingScheduleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A capped offboarding schedule as the offboarding team keeps it.</summary>
public sealed class CappedOffboardingSchedule(string code, string label, bool visible, string locale, int revision, double floorValue, int capacity, string category, int expiresAt, string checksum, string channel, CappedOffboardingScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public string Locale { get; set; } = locale;
    public int Revision { get; set; } = revision;
    public double FloorValue { get; set; } = floorValue;
    public int Capacity { get; set; } = capacity;
    public string Category { get; set; } = category;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Checksum { get; set; } = checksum;
    public string Channel { get; set; } = channel;
    public CappedOffboardingScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a capped offboarding schedule in status requested.</summary>
    public static CappedOffboardingSchedule NewCappedOffboardingSchedule(string code, string label)
    {
        return new CappedOffboardingSchedule(
            code,
            label,
            false,
            "",
            0,
            0.0,
            0,
            "",
            0,
            "",
            "",
            CappedOffboardingScheduleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedOffboardingScheduleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedOffboardingScheduleStatus next)
    {
        switch (Status)
        {
            case CappedOffboardingScheduleStatus.Requested:
                return next == CappedOffboardingScheduleStatus.Granted || next == CappedOffboardingScheduleStatus.Expired;
            case CappedOffboardingScheduleStatus.Granted:
                return next == CappedOffboardingScheduleStatus.Revoked;
            case CappedOffboardingScheduleStatus.Revoked:
                return next == CappedOffboardingScheduleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CappedOffboardingScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped offboarding schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Revision + (double)Capacity, 2);
    }

    public CappedOffboardingSchedule Copy()
    {
        return new CappedOffboardingSchedule(
            Code,
            Label,
            Visible,
            Locale,
            Revision,
            FloorValue,
            Capacity,
            Category,
            ExpiresAt,
            Checksum,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
