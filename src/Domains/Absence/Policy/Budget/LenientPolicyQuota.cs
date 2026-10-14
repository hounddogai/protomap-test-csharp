namespace Acme.Workforce.Absence.Policy.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient policy quota is in its life cycle.</summary>
public enum LenientPolicyQuotaStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a lenient policy quota and its current status.</summary>
public sealed class LenientPolicyQuota(string code, string label, bool approved, string segment, double rating, double ratio, LenientPolicyQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public string Segment { get; set; } = segment;
    public double Rating { get; set; } = rating;
    public double Ratio { get; set; } = ratio;
    public LenientPolicyQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a lenient policy quota in status proposed.</summary>
    public static LenientPolicyQuota NewLenientPolicyQuota(string code, string label)
    {
        return new LenientPolicyQuota(code, label, false, "", 0.0, 0.0, LenientPolicyQuotaStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPolicyQuotaStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPolicyQuotaStatus next)
    {
        switch (Status)
        {
            case LenientPolicyQuotaStatus.Proposed:
                return next == LenientPolicyQuotaStatus.Rejected;
            case LenientPolicyQuotaStatus.Approved:
                switch (next)
                {
                    case LenientPolicyQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
            case LenientPolicyQuotaStatus.Rejected:
                return next == LenientPolicyQuotaStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LenientPolicyQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient policy quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Ratio * Rating, 2);
    }

    public LenientPolicyQuota Copy()
    {
        return new LenientPolicyQuota(Code, Label, Approved, Segment, Rating, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
