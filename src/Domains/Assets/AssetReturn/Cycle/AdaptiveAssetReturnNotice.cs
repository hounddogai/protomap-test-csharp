namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive asset return notice is in its life cycle.</summary>
public enum AdaptiveAssetReturnNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A adaptive asset return notice as the asset return team keeps it.</summary>
public sealed class AdaptiveAssetReturnNotice(string code, string title, string notes, int quantity, string skill, string category, double hours, bool pinned, double threshold, bool active, AdaptiveAssetReturnNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public int Quantity { get; set; } = quantity;
    public string Skill { get; set; } = skill;
    public string Category { get; set; } = category;
    public double Hours { get; set; } = hours;
    public bool Pinned { get; set; } = pinned;
    public double Threshold { get; set; } = threshold;
    public bool Active { get; set; } = active;
    public AdaptiveAssetReturnNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive asset return notice in status created.</summary>
    public static AdaptiveAssetReturnNotice NewAdaptiveAssetReturnNotice(string code, string title)
    {
        return new AdaptiveAssetReturnNotice(
            code,
            title,
            "",
            0,
            "",
            "",
            0.0,
            false,
            0.0,
            false,
            AdaptiveAssetReturnNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAssetReturnNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAssetReturnNoticeStatus next)
    {
        switch (Status)
        {
            case AdaptiveAssetReturnNoticeStatus.Created:
                switch (next)
                {
                    case AdaptiveAssetReturnNoticeStatus.Published:
                        return true;
                }
                return false;
            case AdaptiveAssetReturnNoticeStatus.Validated:
                switch (next)
                {
                    case AdaptiveAssetReturnNoticeStatus.Retired:
                        return true;
                }
                return false;
            case AdaptiveAssetReturnNoticeStatus.Published:
                switch (next)
                {
                    case AdaptiveAssetReturnNoticeStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveAssetReturnNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive asset return notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Hours * (double)Quantity, 2);
    }

    public AdaptiveAssetReturnNotice Copy()
    {
        return new AdaptiveAssetReturnNotice(
            Code,
            Title,
            Notes,
            Quantity,
            Skill,
            Category,
            Hours,
            Pinned,
            Threshold,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
