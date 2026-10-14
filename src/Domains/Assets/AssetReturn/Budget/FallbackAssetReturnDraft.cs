namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback asset return draft is in its life cycle.</summary>
public enum FallbackAssetReturnDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A fallback asset return draft as the asset return team keeps it.</summary>
public sealed class FallbackAssetReturnDraft(string code, string title, string currency, string reviewer, int durationMinutes, string skill, double discount, string locale, double ceiling, FallbackAssetReturnDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public string Reviewer { get; set; } = reviewer;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Skill { get; set; } = skill;
    public double Discount { get; set; } = discount;
    public string Locale { get; set; } = locale;
    public double Ceiling { get; set; } = ceiling;
    public FallbackAssetReturnDraftStatus Status { get; set; } = status;

    /// <summary>Starts a fallback asset return draft in status new.</summary>
    public static FallbackAssetReturnDraft NewFallbackAssetReturnDraft(string code, string title)
    {
        return new FallbackAssetReturnDraft(
            code,
            title,
            "",
            "",
            0,
            "",
            0.0,
            "",
            0.0,
            FallbackAssetReturnDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAssetReturnDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAssetReturnDraftStatus next)
    {
        switch (Status)
        {
            case FallbackAssetReturnDraftStatus.New:
                return next == FallbackAssetReturnDraftStatus.Running || next == FallbackAssetReturnDraftStatus.Done;
            case FallbackAssetReturnDraftStatus.Queued:
                return next == FallbackAssetReturnDraftStatus.Running;
            case FallbackAssetReturnDraftStatus.Running:
                switch (next)
                {
                    case FallbackAssetReturnDraftStatus.Done:
                    case FallbackAssetReturnDraftStatus.Failed:
                        return true;
                }
                return false;
            case FallbackAssetReturnDraftStatus.Done:
                return next == FallbackAssetReturnDraftStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FallbackAssetReturnDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback asset return draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)DurationMinutes - Ceiling, 2);
    }

    public FallbackAssetReturnDraft Copy()
    {
        return new FallbackAssetReturnDraft(
            Code,
            Title,
            Currency,
            Reviewer,
            DurationMinutes,
            Skill,
            Discount,
            Locale,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
