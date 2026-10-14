namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered warranty manifest is in its life cycle.</summary>
public enum TieredWarrantyManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A tiered warranty manifest. Its code is unique within a tenant.</summary>
public sealed class TieredWarrantyManifest(string code, string name, string region, string country, string notes, string contactEmail, int quantity, string title, TieredWarrantyManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public string Country { get; set; } = country;
    public string Notes { get; set; } = notes;
    public string ContactEmail { get; set; } = contactEmail;
    public int Quantity { get; set; } = quantity;
    public string Title { get; set; } = title;
    public TieredWarrantyManifestStatus Status { get; set; } = status;

    /// <summary>Starts a tiered warranty manifest in status proposed.</summary>
    public static TieredWarrantyManifest NewTieredWarrantyManifest(string code, string name)
    {
        return new TieredWarrantyManifest(code, name, "", "", "", "", 0, "", TieredWarrantyManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredWarrantyManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredWarrantyManifestStatus next)
    {
        switch (Status)
        {
            case TieredWarrantyManifestStatus.Proposed:
                return next == TieredWarrantyManifestStatus.Approved || next == TieredWarrantyManifestStatus.Rejected;
            case TieredWarrantyManifestStatus.Approved:
                switch (next)
                {
                    case TieredWarrantyManifestStatus.Withdrawn:
                        return true;
                }
                return false;
            case TieredWarrantyManifestStatus.Rejected:
                switch (next)
                {
                    case TieredWarrantyManifestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredWarrantyManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered warranty manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tiered warranty manifest"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public TieredWarrantyManifest Copy()
    {
        return new TieredWarrantyManifest(Code, Name, Region, Country, Notes, ContactEmail, Quantity, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
