namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return notice is in its life cycle.</summary>
public enum AssetReturnNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One asset return notice, with what the planners need to act on it.</summary>
public sealed class AssetReturnNotice(string code, string title, double fte, string category, int sequence, double discount, string contactEmail, bool approved, double floorValue, AssetReturnNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public string Category { get; set; } = category;
    public int Sequence { get; set; } = sequence;
    public double Discount { get; set; } = discount;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Approved { get; set; } = approved;
    public double FloorValue { get; set; } = floorValue;
    public AssetReturnNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a asset return notice in status created.</summary>
    public static AssetReturnNotice NewAssetReturnNotice(string code, string title)
    {
        return new AssetReturnNotice(code, title, 0.0, "", 0, 0.0, "", false, 0.0, AssetReturnNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnNoticeStatus next)
    {
        switch (Status)
        {
            case AssetReturnNoticeStatus.Created:
                return next == AssetReturnNoticeStatus.Retired;
            case AssetReturnNoticeStatus.Validated:
                return next == AssetReturnNoticeStatus.Published || next == AssetReturnNoticeStatus.Retired;
            case AssetReturnNoticeStatus.Published:
                switch (next)
                {
                    case AssetReturnNoticeStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetLoad()
    {
        return Math.Round(Discount + (double)Sequence, 2);
    }

    public AssetReturnNotice Copy()
    {
        return new AssetReturnNotice(
            Code,
            Title,
            Fte,
            Category,
            Sequence,
            Discount,
            ContactEmail,
            Approved,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
