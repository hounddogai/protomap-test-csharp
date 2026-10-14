namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy action plan digest is in its life cycle.</summary>
public enum LegacyActionPlanDigestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A legacy action plan digest. Its code is unique within a tenant.</summary>
public sealed class LegacyActionPlanDigest(string code, string title, string reviewer, string skill, int limit, LegacyActionPlanDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public string Skill { get; set; } = skill;
    public int Limit { get; set; } = limit;
    public LegacyActionPlanDigestStatus Status { get; set; } = status;

    /// <summary>Starts a legacy action plan digest in status created.</summary>
    public static LegacyActionPlanDigest NewLegacyActionPlanDigest(string code, string title)
    {
        return new LegacyActionPlanDigest(code, title, "", "", 0, LegacyActionPlanDigestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyActionPlanDigestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyActionPlanDigestStatus next)
    {
        switch (Status)
        {
            case LegacyActionPlanDigestStatus.Created:
                switch (next)
                {
                    case LegacyActionPlanDigestStatus.Published:
                    case LegacyActionPlanDigestStatus.Retired:
                        return true;
                }
                return false;
            case LegacyActionPlanDigestStatus.Validated:
                return next == LegacyActionPlanDigestStatus.Published;
            case LegacyActionPlanDigestStatus.Published:
                switch (next)
                {
                    case LegacyActionPlanDigestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyActionPlanDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy action plan digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy action plan digest"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public LegacyActionPlanDigest Copy()
    {
        return new LegacyActionPlanDigest(Code, Title, Reviewer, Skill, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
