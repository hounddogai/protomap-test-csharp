namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return window is in its life cycle.</summary>
public enum AssetReturnWindowStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A asset return window as the asset return team keeps it.</summary>
public sealed class AssetReturnWindow(string code, string title, string country, string currency, Dictionary<string, string> attributes, double rate, int version, double ratio, string name, string notes, AssetReturnWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public string Currency { get; set; } = currency;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rate { get; set; } = rate;
    public int Version { get; set; } = version;
    public double Ratio { get; set; } = ratio;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public AssetReturnWindowStatus Status { get; set; } = status;

    /// <summary>Starts a asset return window in status planned.</summary>
    public static AssetReturnWindow NewAssetReturnWindow(string code, string title)
    {
        return new AssetReturnWindow(
            code,
            title,
            "",
            "",
            new Dictionary<string, string>(),
            0.0,
            0,
            0.0,
            "",
            "",
            AssetReturnWindowStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnWindowStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnWindowStatus next)
    {
        switch (Status)
        {
            case AssetReturnWindowStatus.Planned:
                return next == AssetReturnWindowStatus.Completed || next == AssetReturnWindowStatus.Cancelled;
            case AssetReturnWindowStatus.Scheduled:
                return next == AssetReturnWindowStatus.InProgress || next == AssetReturnWindowStatus.Completed;
            case AssetReturnWindowStatus.InProgress:
                return next == AssetReturnWindowStatus.Completed || next == AssetReturnWindowStatus.Cancelled;
            case AssetReturnWindowStatus.Completed:
                return next == AssetReturnWindowStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AssetReturnWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"asset return window"} {Code}";
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

    public double WeightedExposure()
    {
        return Math.Round((double)Version * Rate, 2);
    }

    public AssetReturnWindow Copy()
    {
        return new AssetReturnWindow(
            Code,
            Title,
            Country,
            Currency,
            Attributes,
            Rate,
            Version,
            Ratio,
            Name,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
