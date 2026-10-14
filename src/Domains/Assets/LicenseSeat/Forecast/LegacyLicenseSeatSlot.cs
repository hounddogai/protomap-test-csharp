namespace Acme.Workforce.Assets.LicenseSeat.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy license seat slot is in its life cycle.</summary>
public enum LegacyLicenseSeatSlotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A legacy license seat slot. Its code is unique within a tenant.</summary>
public sealed class LegacyLicenseSeatSlot(string code, string name, double weight, int revision, string employeeId, string checksum, string timezone, bool archived, string source, string country, int attempts, LegacyLicenseSeatSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public int Revision { get; set; } = revision;
    public string EmployeeId { get; set; } = employeeId;
    public string Checksum { get; set; } = checksum;
    public string Timezone { get; set; } = timezone;
    public bool Archived { get; set; } = archived;
    public string Source { get; set; } = source;
    public string Country { get; set; } = country;
    public int Attempts { get; set; } = attempts;
    public LegacyLicenseSeatSlotStatus Status { get; set; } = status;

    /// <summary>Starts a legacy license seat slot in status draft.</summary>
    public static LegacyLicenseSeatSlot NewLegacyLicenseSeatSlot(string code, string name)
    {
        return new LegacyLicenseSeatSlot(
            code,
            name,
            0.0,
            0,
            "",
            "",
            "",
            false,
            "",
            "",
            0,
            LegacyLicenseSeatSlotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyLicenseSeatSlotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyLicenseSeatSlotStatus next)
    {
        switch (Status)
        {
            case LegacyLicenseSeatSlotStatus.Draft:
                switch (next)
                {
                    case LegacyLicenseSeatSlotStatus.Closed:
                        return true;
                }
                return false;
            case LegacyLicenseSeatSlotStatus.Pending:
                return next == LegacyLicenseSeatSlotStatus.Suspended;
            case LegacyLicenseSeatSlotStatus.Active:
                return next == LegacyLicenseSeatSlotStatus.Closed;
            case LegacyLicenseSeatSlotStatus.Suspended:
                return next == LegacyLicenseSeatSlotStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LegacyLicenseSeatSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy license seat slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"legacy license seat slot"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Weight - (double)Attempts, 2);
    }

    public LegacyLicenseSeatSlot Copy()
    {
        return new LegacyLicenseSeatSlot(
            Code,
            Name,
            Weight,
            Revision,
            EmployeeId,
            Checksum,
            Timezone,
            Archived,
            Source,
            Country,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
