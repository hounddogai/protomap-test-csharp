namespace Acme.Workforce.Absence.ReturnPlan.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan draft is in its life cycle.</summary>
public enum ReturnPlanDraftStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A return plan draft as the return plan team keeps it.</summary>
public sealed class ReturnPlanDraft(string code, string label, string currency, string contactEmail, bool archived, List<string> tags, string description, bool visible, double hours, bool approved, ReturnPlanDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Archived { get; set; } = archived;
    public List<string> Tags { get; set; } = tags;
    public string Description { get; set; } = description;
    public bool Visible { get; set; } = visible;
    public double Hours { get; set; } = hours;
    public bool Approved { get; set; } = approved;
    public ReturnPlanDraftStatus Status { get; set; } = status;

    /// <summary>Starts a return plan draft in status idle.</summary>
    public static ReturnPlanDraft NewReturnPlanDraft(string code, string label)
    {
        return new ReturnPlanDraft(
            code,
            label,
            "",
            "",
            false,
            new List<string>(),
            "",
            false,
            0.0,
            false,
            ReturnPlanDraftStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanDraftStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanDraftStatus next)
    {
        switch (Status)
        {
            case ReturnPlanDraftStatus.Idle:
                return next == ReturnPlanDraftStatus.Warming;
            case ReturnPlanDraftStatus.Warming:
                return next == ReturnPlanDraftStatus.Ready || next == ReturnPlanDraftStatus.Draining;
            case ReturnPlanDraftStatus.Ready:
                return next == ReturnPlanDraftStatus.Stopped;
            case ReturnPlanDraftStatus.Draining:
                return next == ReturnPlanDraftStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ReturnPlanDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double MeasuredValue()
    {
        return Hours;
    }

    public ReturnPlanDraft Copy()
    {
        return new ReturnPlanDraft(
            Code,
            Label,
            Currency,
            ContactEmail,
            Archived,
            Tags,
            Description,
            Visible,
            Hours,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
