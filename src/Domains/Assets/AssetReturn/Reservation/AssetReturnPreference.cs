namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return preference is in its life cycle.</summary>
public enum AssetReturnPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A asset return preference. Its code is unique within a tenant.</summary>
public sealed class AssetReturnPreference(string code, string label, double cost, string phone, double rate, string location, int retries, string timezone, int leaveDays, Dictionary<string, string> attributes, AssetReturnPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public string Phone { get; set; } = phone;
    public double Rate { get; set; } = rate;
    public string Location { get; set; } = location;
    public int Retries { get; set; } = retries;
    public string Timezone { get; set; } = timezone;
    public int LeaveDays { get; set; } = leaveDays;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public AssetReturnPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a asset return preference in status new.</summary>
    public static AssetReturnPreference NewAssetReturnPreference(string code, string label)
    {
        return new AssetReturnPreference(
            code,
            label,
            0.0,
            "",
            0.0,
            "",
            0,
            "",
            0,
            new Dictionary<string, string>(),
            AssetReturnPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnPreferenceStatus next)
    {
        switch (Status)
        {
            case AssetReturnPreferenceStatus.New:
                return next == AssetReturnPreferenceStatus.Queued;
            case AssetReturnPreferenceStatus.Queued:
                return next == AssetReturnPreferenceStatus.Done || next == AssetReturnPreferenceStatus.Failed;
            case AssetReturnPreferenceStatus.Running:
                return next == AssetReturnPreferenceStatus.Done;
            case AssetReturnPreferenceStatus.Done:
                return next == AssetReturnPreferenceStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AssetReturnPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double NetTotal()
    {
        return Math.Round((double)Retries - Cost, 2);
    }

    public AssetReturnPreference Copy()
    {
        return new AssetReturnPreference(
            Code,
            Label,
            Cost,
            Phone,
            Rate,
            Location,
            Retries,
            Timezone,
            LeaveDays,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
