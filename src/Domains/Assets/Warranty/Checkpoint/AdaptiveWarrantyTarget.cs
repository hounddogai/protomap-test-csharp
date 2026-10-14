namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive warranty target is in its life cycle.</summary>
public enum AdaptiveWarrantyTargetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A adaptive warranty target. Its code is unique within a tenant.</summary>
public sealed class AdaptiveWarrantyTarget(string code, string title, int quantity, string description, bool pinned, int attempts, string label, string locale, string category, AdaptiveWarrantyTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string Description { get; set; } = description;
    public bool Pinned { get; set; } = pinned;
    public int Attempts { get; set; } = attempts;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public string Category { get; set; } = category;
    public AdaptiveWarrantyTargetStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive warranty target in status open.</summary>
    public static AdaptiveWarrantyTarget NewAdaptiveWarrantyTarget(string code, string title)
    {
        return new AdaptiveWarrantyTarget(
            code,
            title,
            0,
            "",
            false,
            0,
            "",
            "",
            "",
            AdaptiveWarrantyTargetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveWarrantyTargetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveWarrantyTargetStatus next)
    {
        switch (Status)
        {
            case AdaptiveWarrantyTargetStatus.Open:
                switch (next)
                {
                    case AdaptiveWarrantyTargetStatus.Resolved:
                    case AdaptiveWarrantyTargetStatus.Reopened:
                        return true;
                }
                return false;
            case AdaptiveWarrantyTargetStatus.InReview:
                switch (next)
                {
                    case AdaptiveWarrantyTargetStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveWarrantyTargetStatus.Resolved:
                return next == AdaptiveWarrantyTargetStatus.Reopened || next == AdaptiveWarrantyTargetStatus.Archived;
            case AdaptiveWarrantyTargetStatus.Reopened:
                switch (next)
                {
                    case AdaptiveWarrantyTargetStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveWarrantyTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive warranty target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adaptive warranty target"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Attempts * (double)Quantity, 2);
    }

    public AdaptiveWarrantyTarget Copy()
    {
        return new AdaptiveWarrantyTarget(
            Code,
            Title,
            Quantity,
            Description,
            Pinned,
            Attempts,
            Label,
            Locale,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
