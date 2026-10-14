namespace Acme.Workforce.Contractors.Vendor.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor preference is in its life cycle.</summary>
public enum VendorPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A vendor preference. Its code is unique within a tenant.</summary>
public sealed class VendorPreference(string code, string label, int sequence, int updatedAt, double amount, double floorValue, double maximum, string region, List<string> tags, string reviewer, VendorPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Amount { get; set; } = amount;
    public double FloorValue { get; set; } = floorValue;
    public double Maximum { get; set; } = maximum;
    public string Region { get; set; } = region;
    public List<string> Tags { get; set; } = tags;
    public string Reviewer { get; set; } = reviewer;
    public VendorPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a vendor preference in status new.</summary>
    public static VendorPreference NewVendorPreference(string code, string label)
    {
        return new VendorPreference(
            code,
            label,
            0,
            0,
            0.0,
            0.0,
            0.0,
            "",
            new List<string>(),
            "",
            VendorPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorPreferenceStatus next)
    {
        switch (Status)
        {
            case VendorPreferenceStatus.New:
                switch (next)
                {
                    case VendorPreferenceStatus.Done:
                    case VendorPreferenceStatus.Failed:
                        return true;
                }
                return false;
            case VendorPreferenceStatus.Queued:
                return next == VendorPreferenceStatus.Running || next == VendorPreferenceStatus.Done;
            case VendorPreferenceStatus.Running:
                return next == VendorPreferenceStatus.Done;
            case VendorPreferenceStatus.Done:
                switch (next)
                {
                    case VendorPreferenceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"vendor preference"} {Code}";
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
        return Math.Round(FloorValue - (double)UpdatedAt, 2);
    }

    public VendorPreference Copy()
    {
        return new VendorPreference(
            Code,
            Label,
            Sequence,
            UpdatedAt,
            Amount,
            FloorValue,
            Maximum,
            Region,
            Tags,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
