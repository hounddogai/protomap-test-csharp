namespace Acme.Workforce.Payrollops.Deduction.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict deduction draft is in its life cycle.</summary>
public enum StrictDeductionDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a strict deduction draft and its current status.</summary>
public sealed class StrictDeductionDraft(string code, string title, string description, string ownerId, string unit, int quantity, StrictDeductionDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public string OwnerId { get; set; } = ownerId;
    public string Unit { get; set; } = unit;
    public int Quantity { get; set; } = quantity;
    public StrictDeductionDraftStatus Status { get; set; } = status;

    /// <summary>Starts a strict deduction draft in status requested.</summary>
    public static StrictDeductionDraft NewStrictDeductionDraft(string code, string title)
    {
        return new StrictDeductionDraft(code, title, "", "", "", 0, StrictDeductionDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictDeductionDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictDeductionDraftStatus next)
    {
        switch (Status)
        {
            case StrictDeductionDraftStatus.Requested:
                switch (next)
                {
                    case StrictDeductionDraftStatus.Revoked:
                    case StrictDeductionDraftStatus.Expired:
                        return true;
                }
                return false;
            case StrictDeductionDraftStatus.Granted:
                return next == StrictDeductionDraftStatus.Revoked || next == StrictDeductionDraftStatus.Expired;
            case StrictDeductionDraftStatus.Revoked:
                switch (next)
                {
                    case StrictDeductionDraftStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictDeductionDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict deduction draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public StrictDeductionDraft Copy()
    {
        return new StrictDeductionDraft(Code, Title, Description, OwnerId, Unit, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
