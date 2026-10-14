namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return manifest is in its life cycle.</summary>
public enum AssetReturnManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A asset return manifest as the asset return team keeps it.</summary>
public sealed class AssetReturnManifest(string code, string label, bool approved, double threshold, string timezone, string location, AssetReturnManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public double Threshold { get; set; } = threshold;
    public string Timezone { get; set; } = timezone;
    public string Location { get; set; } = location;
    public AssetReturnManifestStatus Status { get; set; } = status;

    /// <summary>Starts a asset return manifest in status planned.</summary>
    public static AssetReturnManifest NewAssetReturnManifest(string code, string label)
    {
        return new AssetReturnManifest(code, label, false, 0.0, "", "", AssetReturnManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnManifestStatus next)
    {
        switch (Status)
        {
            case AssetReturnManifestStatus.Planned:
                switch (next)
                {
                    case AssetReturnManifestStatus.Scheduled:
                    case AssetReturnManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case AssetReturnManifestStatus.Scheduled:
                switch (next)
                {
                    case AssetReturnManifestStatus.Completed:
                    case AssetReturnManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case AssetReturnManifestStatus.InProgress:
                return next == AssetReturnManifestStatus.Completed;
            case AssetReturnManifestStatus.Completed:
                return next == AssetReturnManifestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AssetReturnManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"asset return manifest"} {Code}";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public AssetReturnManifest Copy()
    {
        return new AssetReturnManifest(Code, Label, Approved, Threshold, Timezone, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
