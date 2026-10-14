namespace Acme.Workforce.Contractors.Vendor.VendorRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback vendor manifest is in its life cycle.</summary>
public enum FallbackVendorManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A fallback vendor manifest as the vendor team keeps it.</summary>
public sealed class FallbackVendorManifest(string code, string label, double ratio, List<string> tags, string name, string ownerId, Dictionary<string, string> attributes, FallbackVendorManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public List<string> Tags { get; set; } = tags;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public FallbackVendorManifestStatus Status { get; set; } = status;

    /// <summary>Starts a fallback vendor manifest in status created.</summary>
    public static FallbackVendorManifest NewFallbackVendorManifest(string code, string label)
    {
        return new FallbackVendorManifest(
            code,
            label,
            0.0,
            new List<string>(),
            "",
            "",
            new Dictionary<string, string>(),
            FallbackVendorManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackVendorManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackVendorManifestStatus next)
    {
        switch (Status)
        {
            case FallbackVendorManifestStatus.Created:
                switch (next)
                {
                    case FallbackVendorManifestStatus.Published:
                        return true;
                }
                return false;
            case FallbackVendorManifestStatus.Validated:
                switch (next)
                {
                    case FallbackVendorManifestStatus.Published:
                    case FallbackVendorManifestStatus.Retired:
                        return true;
                }
                return false;
            case FallbackVendorManifestStatus.Published:
                switch (next)
                {
                    case FallbackVendorManifestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackVendorManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback vendor manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback vendor manifest"} {Code}";
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

    public double MeasuredValue()
    {
        return Ratio;
    }

    public FallbackVendorManifest Copy()
    {
        return new FallbackVendorManifest(Code, Label, Ratio, Tags, Name, OwnerId, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
