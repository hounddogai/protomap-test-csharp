namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional license seat slot is in its life cycle.</summary>
public enum RegionalLicenseSeatSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A regional license seat slot as the license seat team keeps it.</summary>
public sealed class RegionalLicenseSeatSlot(string code, string name, string summary, List<string> tags, double ratio, double floorValue, double weight, double discount, string category, string country, bool active, RegionalLicenseSeatSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public List<string> Tags { get; set; } = tags;
    public double Ratio { get; set; } = ratio;
    public double FloorValue { get; set; } = floorValue;
    public double Weight { get; set; } = weight;
    public double Discount { get; set; } = discount;
    public string Category { get; set; } = category;
    public string Country { get; set; } = country;
    public bool Active { get; set; } = active;
    public RegionalLicenseSeatSlotStatus Status { get; set; } = status;

    /// <summary>Starts a regional license seat slot in status proposed.</summary>
    public static RegionalLicenseSeatSlot NewRegionalLicenseSeatSlot(string code, string name)
    {
        return new RegionalLicenseSeatSlot(
            code,
            name,
            "",
            new List<string>(),
            0.0,
            0.0,
            0.0,
            0.0,
            "",
            "",
            false,
            RegionalLicenseSeatSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalLicenseSeatSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalLicenseSeatSlotStatus next)
    {
        switch (Status)
        {
            case RegionalLicenseSeatSlotStatus.Proposed:
                return next == RegionalLicenseSeatSlotStatus.Approved || next == RegionalLicenseSeatSlotStatus.Rejected;
            case RegionalLicenseSeatSlotStatus.Approved:
                return next == RegionalLicenseSeatSlotStatus.Rejected || next == RegionalLicenseSeatSlotStatus.Withdrawn;
            case RegionalLicenseSeatSlotStatus.Rejected:
                switch (next)
                {
                    case RegionalLicenseSeatSlotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalLicenseSeatSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional license seat slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double AdjustedVolume()
    {
        return Math.Round(Ratio + Weight, 2);
    }

    public RegionalLicenseSeatSlot Copy()
    {
        return new RegionalLicenseSeatSlot(
            Code,
            Name,
            Summary,
            Tags,
            Ratio,
            FloorValue,
            Weight,
            Discount,
            Category,
            Country,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
