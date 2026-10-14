namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive asset return draft is in its life cycle.</summary>
public enum AdaptiveAssetReturnDraftStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A adaptive asset return draft. Its code is unique within a tenant.</summary>
public sealed class AdaptiveAssetReturnDraft(string code, string label, string segment, int createdAt, int capacity, int batchSize, string category, int endsAt, AdaptiveAssetReturnDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public int CreatedAt { get; set; } = createdAt;
    public int Capacity { get; set; } = capacity;
    public int BatchSize { get; set; } = batchSize;
    public string Category { get; set; } = category;
    public int EndsAt { get; set; } = endsAt;
    public AdaptiveAssetReturnDraftStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive asset return draft in status proposed.</summary>
    public static AdaptiveAssetReturnDraft NewAdaptiveAssetReturnDraft(string code, string label)
    {
        return new AdaptiveAssetReturnDraft(
            code,
            label,
            "",
            0,
            0,
            0,
            "",
            0,
            AdaptiveAssetReturnDraftStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAssetReturnDraftStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAssetReturnDraftStatus next)
    {
        switch (Status)
        {
            case AdaptiveAssetReturnDraftStatus.Proposed:
                switch (next)
                {
                    case AdaptiveAssetReturnDraftStatus.Approved:
                        return true;
                }
                return false;
            case AdaptiveAssetReturnDraftStatus.Approved:
                return next == AdaptiveAssetReturnDraftStatus.Rejected;
            case AdaptiveAssetReturnDraftStatus.Rejected:
                return next == AdaptiveAssetReturnDraftStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AdaptiveAssetReturnDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive asset return draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)BatchSize - (double)Capacity, 2);
    }

    public AdaptiveAssetReturnDraft Copy()
    {
        return new AdaptiveAssetReturnDraft(
            Code,
            Label,
            Segment,
            CreatedAt,
            Capacity,
            BatchSize,
            Category,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
