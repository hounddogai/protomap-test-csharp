namespace Acme.Workforce.Assets.AssetReturn.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual asset return tier is in its life cycle.</summary>
public enum ManualAssetReturnTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One manual asset return tier, with what the planners need to act on it.</summary>
public sealed class ManualAssetReturnTier(string code, string title, double amount, int windowDays, string checksum, int createdAt, ManualAssetReturnTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public int WindowDays { get; set; } = windowDays;
    public string Checksum { get; set; } = checksum;
    public int CreatedAt { get; set; } = createdAt;
    public ManualAssetReturnTierStatus Status { get; set; } = status;

    /// <summary>Starts a manual asset return tier in status draft.</summary>
    public static ManualAssetReturnTier NewManualAssetReturnTier(string code, string title)
    {
        return new ManualAssetReturnTier(code, title, 0.0, 0, "", 0, ManualAssetReturnTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAssetReturnTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAssetReturnTierStatus next)
    {
        switch (Status)
        {
            case ManualAssetReturnTierStatus.Draft:
                return next == ManualAssetReturnTierStatus.Closed;
            case ManualAssetReturnTierStatus.Pending:
                return next == ManualAssetReturnTierStatus.Suspended || next == ManualAssetReturnTierStatus.Closed;
            case ManualAssetReturnTierStatus.Active:
                switch (next)
                {
                    case ManualAssetReturnTierStatus.Suspended:
                        return true;
                }
                return false;
            case ManualAssetReturnTierStatus.Suspended:
                switch (next)
                {
                    case ManualAssetReturnTierStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualAssetReturnTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual asset return tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)CreatedAt + Amount, 2);
    }

    public ManualAssetReturnTier Copy()
    {
        return new ManualAssetReturnTier(Code, Title, Amount, WindowDays, Checksum, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
