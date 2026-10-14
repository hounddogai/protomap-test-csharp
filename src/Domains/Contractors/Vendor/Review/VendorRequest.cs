namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor request is in its life cycle.</summary>
public enum VendorRequestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One vendor request, with what the planners need to act on it.</summary>
public sealed class VendorRequest(string code, string label, bool archived, bool visible, int updatedAt, int revision, string department, int version, int priority, double rate, VendorRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public bool Visible { get; set; } = visible;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Revision { get; set; } = revision;
    public string Department { get; set; } = department;
    public int Version { get; set; } = version;
    public int Priority { get; set; } = priority;
    public double Rate { get; set; } = rate;
    public VendorRequestStatus Status { get; set; } = status;

    /// <summary>Starts a vendor request in status open.</summary>
    public static VendorRequest NewVendorRequest(string code, string label)
    {
        return new VendorRequest(code, label, false, false, 0, 0, "", 0, 0, 0.0, VendorRequestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorRequestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorRequestStatus next)
    {
        switch (Status)
        {
            case VendorRequestStatus.Open:
                return next == VendorRequestStatus.Archived;
            case VendorRequestStatus.InReview:
                return next == VendorRequestStatus.Reopened || next == VendorRequestStatus.Archived;
            case VendorRequestStatus.Resolved:
                return next == VendorRequestStatus.Archived;
            case VendorRequestStatus.Reopened:
                switch (next)
                {
                    case VendorRequestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"vendor request"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Priority - (double)UpdatedAt, 2);
    }

    public VendorRequest Copy()
    {
        return new VendorRequest(
            Code,
            Label,
            Archived,
            Visible,
            UpdatedAt,
            Revision,
            Department,
            Version,
            Priority,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
