namespace Acme.Workforce.Contractors.Vendor.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor schedule is in its life cycle.</summary>
public enum VendorScheduleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a vendor schedule and its current status.</summary>
public sealed class VendorSchedule(string code, string name, double floorValue, string country, int updatedAt, bool approved, double maximum, int graceDays, string skill, string grade, VendorScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Country { get; set; } = country;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Approved { get; set; } = approved;
    public double Maximum { get; set; } = maximum;
    public int GraceDays { get; set; } = graceDays;
    public string Skill { get; set; } = skill;
    public string Grade { get; set; } = grade;
    public VendorScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a vendor schedule in status open.</summary>
    public static VendorSchedule NewVendorSchedule(string code, string name)
    {
        return new VendorSchedule(code, name, 0.0, "", 0, false, 0.0, 0, "", "", VendorScheduleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorScheduleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorScheduleStatus next)
    {
        switch (Status)
        {
            case VendorScheduleStatus.Open:
                return next == VendorScheduleStatus.Resolved || next == VendorScheduleStatus.Archived;
            case VendorScheduleStatus.InReview:
                return next == VendorScheduleStatus.Resolved;
            case VendorScheduleStatus.Resolved:
                return next == VendorScheduleStatus.Reopened;
            case VendorScheduleStatus.Reopened:
                return next == VendorScheduleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(VendorScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"vendor schedule"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)GraceDays * Maximum, 2);
    }

    public VendorSchedule Copy()
    {
        return new VendorSchedule(
            Code,
            Name,
            FloorValue,
            Country,
            UpdatedAt,
            Approved,
            Maximum,
            GraceDays,
            Skill,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
