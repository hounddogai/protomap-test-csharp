namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional succession plan draft is in its life cycle.</summary>
public enum RegionalSuccessionPlanDraftStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A regional succession plan draft as the succession plan team keeps it.</summary>
public sealed class RegionalSuccessionPlanDraft(string code, string title, int priority, string reviewer, double threshold, string locale, string segment, double amount, RegionalSuccessionPlanDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public string Reviewer { get; set; } = reviewer;
    public double Threshold { get; set; } = threshold;
    public string Locale { get; set; } = locale;
    public string Segment { get; set; } = segment;
    public double Amount { get; set; } = amount;
    public RegionalSuccessionPlanDraftStatus Status { get; set; } = status;

    /// <summary>Starts a regional succession plan draft in status created.</summary>
    public static RegionalSuccessionPlanDraft NewRegionalSuccessionPlanDraft(string code, string title)
    {
        return new RegionalSuccessionPlanDraft(
            code,
            title,
            0,
            "",
            0.0,
            "",
            "",
            0.0,
            RegionalSuccessionPlanDraftStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSuccessionPlanDraftStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSuccessionPlanDraftStatus next)
    {
        switch (Status)
        {
            case RegionalSuccessionPlanDraftStatus.Created:
                switch (next)
                {
                    case RegionalSuccessionPlanDraftStatus.Validated:
                    case RegionalSuccessionPlanDraftStatus.Published:
                        return true;
                }
                return false;
            case RegionalSuccessionPlanDraftStatus.Validated:
                return next == RegionalSuccessionPlanDraftStatus.Published || next == RegionalSuccessionPlanDraftStatus.Retired;
            case RegionalSuccessionPlanDraftStatus.Published:
                return next == RegionalSuccessionPlanDraftStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RegionalSuccessionPlanDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional succession plan draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional succession plan draft"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Amount * (double)Priority, 2);
    }

    public RegionalSuccessionPlanDraft Copy()
    {
        return new RegionalSuccessionPlanDraft(
            Code,
            Title,
            Priority,
            Reviewer,
            Threshold,
            Locale,
            Segment,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
