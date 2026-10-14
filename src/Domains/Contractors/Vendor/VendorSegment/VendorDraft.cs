namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor draft is in its life cycle.</summary>
public enum VendorDraftStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a vendor draft and its current status.</summary>
public sealed class VendorDraft(string code, string title, bool archived, int attempts, int version, double maximum, string grade, double ratio, int batchSize, string country, VendorDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public int Attempts { get; set; } = attempts;
    public int Version { get; set; } = version;
    public double Maximum { get; set; } = maximum;
    public string Grade { get; set; } = grade;
    public double Ratio { get; set; } = ratio;
    public int BatchSize { get; set; } = batchSize;
    public string Country { get; set; } = country;
    public VendorDraftStatus Status { get; set; } = status;

    /// <summary>Starts a vendor draft in status proposed.</summary>
    public static VendorDraft NewVendorDraft(string code, string title)
    {
        return new VendorDraft(code, title, false, 0, 0, 0.0, "", 0.0, 0, "", VendorDraftStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorDraftStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorDraftStatus next)
    {
        switch (Status)
        {
            case VendorDraftStatus.Proposed:
                return next == VendorDraftStatus.Approved || next == VendorDraftStatus.Rejected;
            case VendorDraftStatus.Approved:
                switch (next)
                {
                    case VendorDraftStatus.Rejected:
                        return true;
                }
                return false;
            case VendorDraftStatus.Rejected:
                switch (next)
                {
                    case VendorDraftStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedExposure()
    {
        return Math.Round(Ratio - (double)Version, 2);
    }

    public VendorDraft Copy()
    {
        return new VendorDraft(
            Code,
            Title,
            Archived,
            Attempts,
            Version,
            Maximum,
            Grade,
            Ratio,
            BatchSize,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
