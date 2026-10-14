namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped deduction assignment is in its life cycle.</summary>
public enum CappedDeductionAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a capped deduction assignment and its current status.</summary>
public sealed class CappedDeductionAssignment(string code, string label, bool active, Dictionary<string, string> attributes, double maximum, int attempts, CappedDeductionAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Maximum { get; set; } = maximum;
    public int Attempts { get; set; } = attempts;
    public CappedDeductionAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a capped deduction assignment in status open.</summary>
    public static CappedDeductionAssignment NewCappedDeductionAssignment(string code, string label)
    {
        return new CappedDeductionAssignment(
            code,
            label,
            false,
            new Dictionary<string, string>(),
            0.0,
            0,
            CappedDeductionAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedDeductionAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedDeductionAssignmentStatus next)
    {
        switch (Status)
        {
            case CappedDeductionAssignmentStatus.Open:
                return next == CappedDeductionAssignmentStatus.Archived;
            case CappedDeductionAssignmentStatus.InReview:
                switch (next)
                {
                    case CappedDeductionAssignmentStatus.Resolved:
                    case CappedDeductionAssignmentStatus.Reopened:
                        return true;
                }
                return false;
            case CappedDeductionAssignmentStatus.Resolved:
                return next == CappedDeductionAssignmentStatus.Reopened || next == CappedDeductionAssignmentStatus.Archived;
            case CappedDeductionAssignmentStatus.Reopened:
                switch (next)
                {
                    case CappedDeductionAssignmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedDeductionAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped deduction assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedLoad()
    {
        return Math.Round(Maximum - (double)Attempts, 2);
    }

    public CappedDeductionAssignment Copy()
    {
        return new CappedDeductionAssignment(Code, Label, Active, Attributes, Maximum, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
