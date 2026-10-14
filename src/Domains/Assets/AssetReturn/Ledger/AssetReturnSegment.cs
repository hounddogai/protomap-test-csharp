namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return segment is in its life cycle.</summary>
public enum AssetReturnSegmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One asset return segment, with what the planners need to act on it.</summary>
public sealed class AssetReturnSegment(string code, string name, string category, double rate, string grade, double discount, double ceiling, string description, string ownerId, double weight, string checksum, AssetReturnSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public double Rate { get; set; } = rate;
    public string Grade { get; set; } = grade;
    public double Discount { get; set; } = discount;
    public double Ceiling { get; set; } = ceiling;
    public string Description { get; set; } = description;
    public string OwnerId { get; set; } = ownerId;
    public double Weight { get; set; } = weight;
    public string Checksum { get; set; } = checksum;
    public AssetReturnSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a asset return segment in status draft.</summary>
    public static AssetReturnSegment NewAssetReturnSegment(string code, string name)
    {
        return new AssetReturnSegment(
            code,
            name,
            "",
            0.0,
            "",
            0.0,
            0.0,
            "",
            "",
            0.0,
            "",
            AssetReturnSegmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnSegmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnSegmentStatus next)
    {
        switch (Status)
        {
            case AssetReturnSegmentStatus.Draft:
                return next == AssetReturnSegmentStatus.Active;
            case AssetReturnSegmentStatus.Pending:
                return next == AssetReturnSegmentStatus.Active || next == AssetReturnSegmentStatus.Suspended;
            case AssetReturnSegmentStatus.Active:
                switch (next)
                {
                    case AssetReturnSegmentStatus.Closed:
                        return true;
                }
                return false;
            case AssetReturnSegmentStatus.Suspended:
                switch (next)
                {
                    case AssetReturnSegmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"asset return segment"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Ceiling - Discount, 2);
    }

    public AssetReturnSegment Copy()
    {
        return new AssetReturnSegment(
            Code,
            Name,
            Category,
            Rate,
            Grade,
            Discount,
            Ceiling,
            Description,
            OwnerId,
            Weight,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
