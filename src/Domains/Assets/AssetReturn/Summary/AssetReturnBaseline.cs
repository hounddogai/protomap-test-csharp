namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return baseline is in its life cycle.</summary>
public enum AssetReturnBaselineStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A asset return baseline. Its code is unique within a tenant.</summary>
public sealed class AssetReturnBaseline(string code, string title, string department, int version, bool approved, int quantity, string salaryBand, int startsAt, double minimum, AssetReturnBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public int Version { get; set; } = version;
    public bool Approved { get; set; } = approved;
    public int Quantity { get; set; } = quantity;
    public string SalaryBand { get; set; } = salaryBand;
    public int StartsAt { get; set; } = startsAt;
    public double Minimum { get; set; } = minimum;
    public AssetReturnBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a asset return baseline in status open.</summary>
    public static AssetReturnBaseline NewAssetReturnBaseline(string code, string title)
    {
        return new AssetReturnBaseline(code, title, "", 0, false, 0, "", 0, 0.0, AssetReturnBaselineStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnBaselineStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnBaselineStatus next)
    {
        switch (Status)
        {
            case AssetReturnBaselineStatus.Open:
                return next == AssetReturnBaselineStatus.InReview || next == AssetReturnBaselineStatus.Archived;
            case AssetReturnBaselineStatus.InReview:
                switch (next)
                {
                    case AssetReturnBaselineStatus.Archived:
                        return true;
                }
                return false;
            case AssetReturnBaselineStatus.Resolved:
                switch (next)
                {
                    case AssetReturnBaselineStatus.Archived:
                        return true;
                }
                return false;
            case AssetReturnBaselineStatus.Reopened:
                return next == AssetReturnBaselineStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AssetReturnBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Quantity + (double)StartsAt, 2);
    }

    public AssetReturnBaseline Copy()
    {
        return new AssetReturnBaseline(
            Code,
            Title,
            Department,
            Version,
            Approved,
            Quantity,
            SalaryBand,
            StartsAt,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
