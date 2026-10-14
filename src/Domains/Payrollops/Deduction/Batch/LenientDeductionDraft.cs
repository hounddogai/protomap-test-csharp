namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient deduction draft is in its life cycle.</summary>
public enum LenientDeductionDraftStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One lenient deduction draft, with what the planners need to act on it.</summary>
public sealed class LenientDeductionDraft(string code, string name, double floorValue, int revision, string category, string description, double ceiling, int priority, int graceDays, string reviewer, LenientDeductionDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public int Revision { get; set; } = revision;
    public string Category { get; set; } = category;
    public string Description { get; set; } = description;
    public double Ceiling { get; set; } = ceiling;
    public int Priority { get; set; } = priority;
    public int GraceDays { get; set; } = graceDays;
    public string Reviewer { get; set; } = reviewer;
    public LenientDeductionDraftStatus Status { get; set; } = status;

    /// <summary>Starts a lenient deduction draft in status created.</summary>
    public static LenientDeductionDraft NewLenientDeductionDraft(string code, string name)
    {
        return new LenientDeductionDraft(
            code,
            name,
            0.0,
            0,
            "",
            "",
            0.0,
            0,
            0,
            "",
            LenientDeductionDraftStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientDeductionDraftStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientDeductionDraftStatus next)
    {
        switch (Status)
        {
            case LenientDeductionDraftStatus.Created:
                switch (next)
                {
                    case LenientDeductionDraftStatus.Validated:
                    case LenientDeductionDraftStatus.Retired:
                        return true;
                }
                return false;
            case LenientDeductionDraftStatus.Validated:
                return next == LenientDeductionDraftStatus.Published || next == LenientDeductionDraftStatus.Retired;
            case LenientDeductionDraftStatus.Published:
                return next == LenientDeductionDraftStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LenientDeductionDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient deduction draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"lenient deduction draft"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Priority - (double)Revision, 2);
    }

    public LenientDeductionDraft Copy()
    {
        return new LenientDeductionDraft(
            Code,
            Name,
            FloorValue,
            Revision,
            Category,
            Description,
            Ceiling,
            Priority,
            GraceDays,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
