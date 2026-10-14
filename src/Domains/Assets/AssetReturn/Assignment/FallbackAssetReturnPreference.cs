namespace Acme.Workforce.Assets.AssetReturn.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback asset return preference is in its life cycle.</summary>
public enum FallbackAssetReturnPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A fallback asset return preference. Its code is unique within a tenant.</summary>
public sealed class FallbackAssetReturnPreference(string code, string label, string title, bool pinned, bool archived, string location, int capacity, int attempts, double margin, int version, FallbackAssetReturnPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public bool Archived { get; set; } = archived;
    public string Location { get; set; } = location;
    public int Capacity { get; set; } = capacity;
    public int Attempts { get; set; } = attempts;
    public double Margin { get; set; } = margin;
    public int Version { get; set; } = version;
    public FallbackAssetReturnPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a fallback asset return preference in status created.</summary>
    public static FallbackAssetReturnPreference NewFallbackAssetReturnPreference(string code, string label)
    {
        return new FallbackAssetReturnPreference(
            code,
            label,
            "",
            false,
            false,
            "",
            0,
            0,
            0.0,
            0,
            FallbackAssetReturnPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAssetReturnPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAssetReturnPreferenceStatus next)
    {
        switch (Status)
        {
            case FallbackAssetReturnPreferenceStatus.Created:
                switch (next)
                {
                    case FallbackAssetReturnPreferenceStatus.Published:
                    case FallbackAssetReturnPreferenceStatus.Retired:
                        return true;
                }
                return false;
            case FallbackAssetReturnPreferenceStatus.Validated:
                switch (next)
                {
                    case FallbackAssetReturnPreferenceStatus.Retired:
                        return true;
                }
                return false;
            case FallbackAssetReturnPreferenceStatus.Published:
                return next == FallbackAssetReturnPreferenceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FallbackAssetReturnPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback asset return preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"fallback asset return preference"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Capacity + (double)Attempts, 2);
    }

    public FallbackAssetReturnPreference Copy()
    {
        return new FallbackAssetReturnPreference(
            Code,
            Label,
            Title,
            Pinned,
            Archived,
            Location,
            Capacity,
            Attempts,
            Margin,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
