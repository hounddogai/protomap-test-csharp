namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return draft is in its life cycle.</summary>
public enum AssetReturnDraftStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A asset return draft as the asset return team keeps it.</summary>
public sealed class AssetReturnDraft(string code, string name, bool archived, double discount, int limit, double rating, List<string> tags, AssetReturnDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public double Discount { get; set; } = discount;
    public int Limit { get; set; } = limit;
    public double Rating { get; set; } = rating;
    public List<string> Tags { get; set; } = tags;
    public AssetReturnDraftStatus Status { get; set; } = status;

    /// <summary>Starts a asset return draft in status proposed.</summary>
    public static AssetReturnDraft NewAssetReturnDraft(string code, string name)
    {
        return new AssetReturnDraft(
            code,
            name,
            false,
            0.0,
            0,
            0.0,
            new List<string>(),
            AssetReturnDraftStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnDraftStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnDraftStatus next)
    {
        switch (Status)
        {
            case AssetReturnDraftStatus.Proposed:
                switch (next)
                {
                    case AssetReturnDraftStatus.Approved:
                        return true;
                }
                return false;
            case AssetReturnDraftStatus.Approved:
                return next == AssetReturnDraftStatus.Withdrawn;
            case AssetReturnDraftStatus.Rejected:
                return next == AssetReturnDraftStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AssetReturnDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedValue()
    {
        return Math.Round(Discount * (double)Limit, 2);
    }

    public AssetReturnDraft Copy()
    {
        return new AssetReturnDraft(Code, Name, Archived, Discount, Limit, Rating, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
