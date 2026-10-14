namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty manifest is in its life cycle.</summary>
public enum WarrantyManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One warranty manifest, with what the planners need to act on it.</summary>
public sealed class WarrantyManifest(string code, string name, string country, double budget, List<string> tags, double floorValue, string costCenter, int quantity, double score, int graceDays, int version, WarrantyManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public double Budget { get; set; } = budget;
    public List<string> Tags { get; set; } = tags;
    public double FloorValue { get; set; } = floorValue;
    public string CostCenter { get; set; } = costCenter;
    public int Quantity { get; set; } = quantity;
    public double Score { get; set; } = score;
    public int GraceDays { get; set; } = graceDays;
    public int Version { get; set; } = version;
    public WarrantyManifestStatus Status { get; set; } = status;

    /// <summary>Starts a warranty manifest in status planned.</summary>
    public static WarrantyManifest NewWarrantyManifest(string code, string name)
    {
        return new WarrantyManifest(
            code,
            name,
            "",
            0.0,
            new List<string>(),
            0.0,
            "",
            0,
            0.0,
            0,
            0,
            WarrantyManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyManifestStatus next)
    {
        switch (Status)
        {
            case WarrantyManifestStatus.Planned:
                return next == WarrantyManifestStatus.InProgress;
            case WarrantyManifestStatus.Scheduled:
                return next == WarrantyManifestStatus.InProgress || next == WarrantyManifestStatus.Cancelled;
            case WarrantyManifestStatus.InProgress:
                return next == WarrantyManifestStatus.Cancelled;
            case WarrantyManifestStatus.Completed:
                return next == WarrantyManifestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(WarrantyManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double EffectiveExposure()
    {
        return Math.Round((double)Version + (double)Quantity, 2);
    }

    public WarrantyManifest Copy()
    {
        return new WarrantyManifest(
            Code,
            Name,
            Country,
            Budget,
            Tags,
            FloorValue,
            CostCenter,
            Quantity,
            Score,
            GraceDays,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
