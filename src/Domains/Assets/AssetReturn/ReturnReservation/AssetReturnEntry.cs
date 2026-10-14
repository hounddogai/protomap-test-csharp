namespace Acme.Workforce.Assets.AssetReturn.ReturnReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return entry is in its life cycle.</summary>
public enum AssetReturnEntryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One asset return entry, with what the planners need to act on it.</summary>
public sealed class AssetReturnEntry(string code, string title, double discount, int createdAt, double maximum, string summary, string currency, int version, string externalRef, AssetReturnEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public int CreatedAt { get; set; } = createdAt;
    public double Maximum { get; set; } = maximum;
    public string Summary { get; set; } = summary;
    public string Currency { get; set; } = currency;
    public int Version { get; set; } = version;
    public string ExternalRef { get; set; } = externalRef;
    public AssetReturnEntryStatus Status { get; set; } = status;

    /// <summary>Starts a asset return entry in status open.</summary>
    public static AssetReturnEntry NewAssetReturnEntry(string code, string title)
    {
        return new AssetReturnEntry(code, title, 0.0, 0, 0.0, "", "", 0, "", AssetReturnEntryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnEntryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnEntryStatus next)
    {
        switch (Status)
        {
            case AssetReturnEntryStatus.Open:
                switch (next)
                {
                    case AssetReturnEntryStatus.InReview:
                    case AssetReturnEntryStatus.Reopened:
                        return true;
                }
                return false;
            case AssetReturnEntryStatus.InReview:
                switch (next)
                {
                    case AssetReturnEntryStatus.Resolved:
                    case AssetReturnEntryStatus.Reopened:
                        return true;
                }
                return false;
            case AssetReturnEntryStatus.Resolved:
                switch (next)
                {
                    case AssetReturnEntryStatus.Reopened:
                        return true;
                }
                return false;
            case AssetReturnEntryStatus.Reopened:
                return next == AssetReturnEntryStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AssetReturnEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)CreatedAt * (double)Version, 2);
    }

    public AssetReturnEntry Copy()
    {
        return new AssetReturnEntry(
            Code,
            Title,
            Discount,
            CreatedAt,
            Maximum,
            Summary,
            Currency,
            Version,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
