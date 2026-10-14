namespace Acme.Workforce.Contractors.Vendor.VendorTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending vendor manifest is in its life cycle.</summary>
public enum PendingVendorManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One pending vendor manifest, with what the planners need to act on it.</summary>
public sealed class PendingVendorManifest(string code, string label, string phone, string region, int startsAt, string externalRef, double discount, PendingVendorManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public string Region { get; set; } = region;
    public int StartsAt { get; set; } = startsAt;
    public string ExternalRef { get; set; } = externalRef;
    public double Discount { get; set; } = discount;
    public PendingVendorManifestStatus Status { get; set; } = status;

    /// <summary>Starts a pending vendor manifest in status planned.</summary>
    public static PendingVendorManifest NewPendingVendorManifest(string code, string label)
    {
        return new PendingVendorManifest(code, label, "", "", 0, "", 0.0, PendingVendorManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingVendorManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingVendorManifestStatus next)
    {
        switch (Status)
        {
            case PendingVendorManifestStatus.Planned:
                return next == PendingVendorManifestStatus.Cancelled;
            case PendingVendorManifestStatus.Scheduled:
                switch (next)
                {
                    case PendingVendorManifestStatus.Completed:
                    case PendingVendorManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingVendorManifestStatus.InProgress:
                switch (next)
                {
                    case PendingVendorManifestStatus.Completed:
                        return true;
                }
                return false;
            case PendingVendorManifestStatus.Completed:
                switch (next)
                {
                    case PendingVendorManifestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingVendorManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending vendor manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"pending vendor manifest"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)StartsAt - Discount, 2);
    }

    public PendingVendorManifest Copy()
    {
        return new PendingVendorManifest(Code, Label, Phone, Region, StartsAt, ExternalRef, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
