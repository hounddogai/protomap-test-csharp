namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped asset return manifest is in its life cycle.</summary>
public enum CappedAssetReturnManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a capped asset return manifest and its current status.</summary>
public sealed class CappedAssetReturnManifest(string code, string title, bool visible, string region, string notes, double hours, CappedAssetReturnManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public string Region { get; set; } = region;
    public string Notes { get; set; } = notes;
    public double Hours { get; set; } = hours;
    public CappedAssetReturnManifestStatus Status { get; set; } = status;

    /// <summary>Starts a capped asset return manifest in status created.</summary>
    public static CappedAssetReturnManifest NewCappedAssetReturnManifest(string code, string title)
    {
        return new CappedAssetReturnManifest(
            code,
            title,
            false,
            "",
            "",
            0.0,
            CappedAssetReturnManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAssetReturnManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAssetReturnManifestStatus next)
    {
        switch (Status)
        {
            case CappedAssetReturnManifestStatus.Created:
                switch (next)
                {
                    case CappedAssetReturnManifestStatus.Published:
                    case CappedAssetReturnManifestStatus.Retired:
                        return true;
                }
                return false;
            case CappedAssetReturnManifestStatus.Validated:
                switch (next)
                {
                    case CappedAssetReturnManifestStatus.Published:
                    case CappedAssetReturnManifestStatus.Retired:
                        return true;
                }
                return false;
            case CappedAssetReturnManifestStatus.Published:
                switch (next)
                {
                    case CappedAssetReturnManifestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedAssetReturnManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped asset return manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Hours;
    }

    public CappedAssetReturnManifest Copy()
    {
        return new CappedAssetReturnManifest(Code, Title, Visible, Region, Notes, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
