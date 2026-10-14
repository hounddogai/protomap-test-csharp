namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped action plan draft is in its life cycle.</summary>
public enum CappedActionPlanDraftStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A capped action plan draft. Its code is unique within a tenant.</summary>
public sealed class CappedActionPlanDraft(string code, string name, string grade, string checksum, string source, bool archived, string ownerId, double amount, CappedActionPlanDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public string Checksum { get; set; } = checksum;
    public string Source { get; set; } = source;
    public bool Archived { get; set; } = archived;
    public string OwnerId { get; set; } = ownerId;
    public double Amount { get; set; } = amount;
    public CappedActionPlanDraftStatus Status { get; set; } = status;

    /// <summary>Starts a capped action plan draft in status created.</summary>
    public static CappedActionPlanDraft NewCappedActionPlanDraft(string code, string name)
    {
        return new CappedActionPlanDraft(
            code,
            name,
            "",
            "",
            "",
            false,
            "",
            0.0,
            CappedActionPlanDraftStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedActionPlanDraftStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedActionPlanDraftStatus next)
    {
        switch (Status)
        {
            case CappedActionPlanDraftStatus.Created:
                switch (next)
                {
                    case CappedActionPlanDraftStatus.Validated:
                    case CappedActionPlanDraftStatus.Published:
                        return true;
                }
                return false;
            case CappedActionPlanDraftStatus.Validated:
                return next == CappedActionPlanDraftStatus.Published;
            case CappedActionPlanDraftStatus.Published:
                return next == CappedActionPlanDraftStatus.Retired;
        }
        return false;
    }

    public void MoveTo(CappedActionPlanDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped action plan draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public CappedActionPlanDraft Copy()
    {
        return new CappedActionPlanDraft(Code, Name, Grade, Checksum, Source, Archived, OwnerId, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
