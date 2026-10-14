namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan draft is in its life cycle.</summary>
public enum SuccessionPlanDraftStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A succession plan draft as the succession plan team keeps it.</summary>
public sealed class SuccessionPlanDraft(string code, string title, double ratio, string reviewer, string phone, string region, string jobCode, double ceiling, List<string> tags, SuccessionPlanDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Reviewer { get; set; } = reviewer;
    public string Phone { get; set; } = phone;
    public string Region { get; set; } = region;
    public string JobCode { get; set; } = jobCode;
    public double Ceiling { get; set; } = ceiling;
    public List<string> Tags { get; set; } = tags;
    public SuccessionPlanDraftStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan draft in status created.</summary>
    public static SuccessionPlanDraft NewSuccessionPlanDraft(string code, string title)
    {
        return new SuccessionPlanDraft(
            code,
            title,
            0.0,
            "",
            "",
            "",
            "",
            0.0,
            new List<string>(),
            SuccessionPlanDraftStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanDraftStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanDraftStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanDraftStatus.Created:
                return next == SuccessionPlanDraftStatus.Retired;
            case SuccessionPlanDraftStatus.Validated:
                return next == SuccessionPlanDraftStatus.Published || next == SuccessionPlanDraftStatus.Retired;
            case SuccessionPlanDraftStatus.Published:
                switch (next)
                {
                    case SuccessionPlanDraftStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double AdjustedExposure()
    {
        return Math.Round(Ceiling * Ratio, 2);
    }

    public SuccessionPlanDraft Copy()
    {
        return new SuccessionPlanDraft(Code, Title, Ratio, Reviewer, Phone, Region, JobCode, Ceiling, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
