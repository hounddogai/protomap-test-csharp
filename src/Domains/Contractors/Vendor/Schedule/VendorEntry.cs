namespace Acme.Workforce.Contractors.Vendor.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor entry is in its life cycle.</summary>
public enum VendorEntryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A vendor entry. Its code is unique within a tenant.</summary>
public sealed class VendorEntry(string code, string title, double ratio, int startsAt, string grade, bool archived, int revision, string externalRef, string timezone, string name, int leaveDays, VendorEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public int StartsAt { get; set; } = startsAt;
    public string Grade { get; set; } = grade;
    public bool Archived { get; set; } = archived;
    public int Revision { get; set; } = revision;
    public string ExternalRef { get; set; } = externalRef;
    public string Timezone { get; set; } = timezone;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public VendorEntryStatus Status { get; set; } = status;

    /// <summary>Starts a vendor entry in status created.</summary>
    public static VendorEntry NewVendorEntry(string code, string title)
    {
        return new VendorEntry(code, title, 0.0, 0, "", false, 0, "", "", "", 0, VendorEntryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorEntryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorEntryStatus next)
    {
        switch (Status)
        {
            case VendorEntryStatus.Created:
                switch (next)
                {
                    case VendorEntryStatus.Validated:
                        return true;
                }
                return false;
            case VendorEntryStatus.Validated:
                return next == VendorEntryStatus.Published;
            case VendorEntryStatus.Published:
                switch (next)
                {
                    case VendorEntryStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)StartsAt - Ratio, 2);
    }

    public VendorEntry Copy()
    {
        return new VendorEntry(
            Code,
            Title,
            Ratio,
            StartsAt,
            Grade,
            Archived,
            Revision,
            ExternalRef,
            Timezone,
            Name,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
