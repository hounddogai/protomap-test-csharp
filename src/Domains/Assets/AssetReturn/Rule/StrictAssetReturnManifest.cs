namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict asset return manifest is in its life cycle.</summary>
public enum StrictAssetReturnManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A strict asset return manifest as the asset return team keeps it.</summary>
public sealed class StrictAssetReturnManifest(string code, string name, double budget, Dictionary<string, string> attributes, double minimum, double amount, StrictAssetReturnManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Minimum { get; set; } = minimum;
    public double Amount { get; set; } = amount;
    public StrictAssetReturnManifestStatus Status { get; set; } = status;

    /// <summary>Starts a strict asset return manifest in status planned.</summary>
    public static StrictAssetReturnManifest NewStrictAssetReturnManifest(string code, string name)
    {
        return new StrictAssetReturnManifest(
            code,
            name,
            0.0,
            new Dictionary<string, string>(),
            0.0,
            0.0,
            StrictAssetReturnManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAssetReturnManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAssetReturnManifestStatus next)
    {
        switch (Status)
        {
            case StrictAssetReturnManifestStatus.Planned:
                switch (next)
                {
                    case StrictAssetReturnManifestStatus.Scheduled:
                    case StrictAssetReturnManifestStatus.Completed:
                        return true;
                }
                return false;
            case StrictAssetReturnManifestStatus.Scheduled:
                return next == StrictAssetReturnManifestStatus.InProgress || next == StrictAssetReturnManifestStatus.Cancelled;
            case StrictAssetReturnManifestStatus.InProgress:
                switch (next)
                {
                    case StrictAssetReturnManifestStatus.Completed:
                    case StrictAssetReturnManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case StrictAssetReturnManifestStatus.Completed:
                switch (next)
                {
                    case StrictAssetReturnManifestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictAssetReturnManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict asset return manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict asset return manifest"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedValue()
    {
        return Math.Round(Minimum - Budget, 2);
    }

    public StrictAssetReturnManifest Copy()
    {
        return new StrictAssetReturnManifest(Code, Name, Budget, Attributes, Minimum, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
