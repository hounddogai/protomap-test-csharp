namespace Acme.Workforce.Assets.AssetReturn.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual asset return baseline is in its life cycle.</summary>
public enum ManualAssetReturnBaselineStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A manual asset return baseline. Its code is unique within a tenant.</summary>
public sealed class ManualAssetReturnBaseline(string code, string title, double cost, string ownerId, int createdAt, double weight, string name, ManualAssetReturnBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public string OwnerId { get; set; } = ownerId;
    public int CreatedAt { get; set; } = createdAt;
    public double Weight { get; set; } = weight;
    public string Name { get; set; } = name;
    public ManualAssetReturnBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a manual asset return baseline in status draft.</summary>
    public static ManualAssetReturnBaseline NewManualAssetReturnBaseline(string code, string title)
    {
        return new ManualAssetReturnBaseline(
            code,
            title,
            0.0,
            "",
            0,
            0.0,
            "",
            ManualAssetReturnBaselineStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAssetReturnBaselineStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAssetReturnBaselineStatus next)
    {
        switch (Status)
        {
            case ManualAssetReturnBaselineStatus.Draft:
                return next == ManualAssetReturnBaselineStatus.Pending;
            case ManualAssetReturnBaselineStatus.Pending:
                return next == ManualAssetReturnBaselineStatus.Suspended || next == ManualAssetReturnBaselineStatus.Closed;
            case ManualAssetReturnBaselineStatus.Active:
                return next == ManualAssetReturnBaselineStatus.Closed;
            case ManualAssetReturnBaselineStatus.Suspended:
                return next == ManualAssetReturnBaselineStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ManualAssetReturnBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual asset return baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)CreatedAt + Weight, 2);
    }

    public ManualAssetReturnBaseline Copy()
    {
        return new ManualAssetReturnBaseline(Code, Title, Cost, OwnerId, CreatedAt, Weight, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
