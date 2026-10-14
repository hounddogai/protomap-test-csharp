namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return profile is in its life cycle.</summary>
public enum AssetReturnProfileStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A asset return profile. Its code is unique within a tenant.</summary>
public sealed class AssetReturnProfile(string code, string title, string reviewer, double margin, double discount, double fte, string currency, int attempts, int updatedAt, double ratio, string location, AssetReturnProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public double Margin { get; set; } = margin;
    public double Discount { get; set; } = discount;
    public double Fte { get; set; } = fte;
    public string Currency { get; set; } = currency;
    public int Attempts { get; set; } = attempts;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Ratio { get; set; } = ratio;
    public string Location { get; set; } = location;
    public AssetReturnProfileStatus Status { get; set; } = status;

    /// <summary>Starts a asset return profile in status proposed.</summary>
    public static AssetReturnProfile NewAssetReturnProfile(string code, string title)
    {
        return new AssetReturnProfile(
            code,
            title,
            "",
            0.0,
            0.0,
            0.0,
            "",
            0,
            0,
            0.0,
            "",
            AssetReturnProfileStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnProfileStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnProfileStatus next)
    {
        switch (Status)
        {
            case AssetReturnProfileStatus.Proposed:
                switch (next)
                {
                    case AssetReturnProfileStatus.Approved:
                        return true;
                }
                return false;
            case AssetReturnProfileStatus.Approved:
                return next == AssetReturnProfileStatus.Rejected || next == AssetReturnProfileStatus.Withdrawn;
            case AssetReturnProfileStatus.Rejected:
                switch (next)
                {
                    case AssetReturnProfileStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Margin * Ratio, 2);
    }

    public AssetReturnProfile Copy()
    {
        return new AssetReturnProfile(
            Code,
            Title,
            Reviewer,
            Margin,
            Discount,
            Fte,
            Currency,
            Attempts,
            UpdatedAt,
            Ratio,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
