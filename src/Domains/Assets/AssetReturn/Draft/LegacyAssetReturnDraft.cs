namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy asset return draft is in its life cycle.</summary>
public enum LegacyAssetReturnDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a legacy asset return draft and its current status.</summary>
public sealed class LegacyAssetReturnDraft(string code, string name, bool pinned, int updatedAt, string category, int limit, Dictionary<string, string> attributes, int startsAt, int retries, LegacyAssetReturnDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Category { get; set; } = category;
    public int Limit { get; set; } = limit;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int StartsAt { get; set; } = startsAt;
    public int Retries { get; set; } = retries;
    public LegacyAssetReturnDraftStatus Status { get; set; } = status;

    /// <summary>Starts a legacy asset return draft in status open.</summary>
    public static LegacyAssetReturnDraft NewLegacyAssetReturnDraft(string code, string name)
    {
        return new LegacyAssetReturnDraft(
            code,
            name,
            false,
            0,
            "",
            0,
            new Dictionary<string, string>(),
            0,
            0,
            LegacyAssetReturnDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyAssetReturnDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyAssetReturnDraftStatus next)
    {
        switch (Status)
        {
            case LegacyAssetReturnDraftStatus.Open:
                switch (next)
                {
                    case LegacyAssetReturnDraftStatus.InReview:
                    case LegacyAssetReturnDraftStatus.Resolved:
                        return true;
                }
                return false;
            case LegacyAssetReturnDraftStatus.InReview:
                switch (next)
                {
                    case LegacyAssetReturnDraftStatus.Reopened:
                        return true;
                }
                return false;
            case LegacyAssetReturnDraftStatus.Resolved:
                return next == LegacyAssetReturnDraftStatus.Reopened || next == LegacyAssetReturnDraftStatus.Archived;
            case LegacyAssetReturnDraftStatus.Reopened:
                switch (next)
                {
                    case LegacyAssetReturnDraftStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyAssetReturnDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy asset return draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double EffectiveTotal()
    {
        return Math.Round((double)Retries + (double)Limit, 2);
    }

    public LegacyAssetReturnDraft Copy()
    {
        return new LegacyAssetReturnDraft(
            Code,
            Name,
            Pinned,
            UpdatedAt,
            Category,
            Limit,
            Attributes,
            StartsAt,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
