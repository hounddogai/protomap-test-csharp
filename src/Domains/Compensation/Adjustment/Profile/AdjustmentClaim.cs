namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment claim is in its life cycle.</summary>
public enum AdjustmentClaimStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A adjustment claim as the adjustment team keeps it.</summary>
public sealed class AdjustmentClaim(string code, string label, string locale, string skill, string unit, double score, AdjustmentClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public string Skill { get; set; } = skill;
    public string Unit { get; set; } = unit;
    public double Score { get; set; } = score;
    public AdjustmentClaimStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment claim in status planned.</summary>
    public static AdjustmentClaim NewAdjustmentClaim(string code, string label)
    {
        return new AdjustmentClaim(code, label, "", "", "", 0.0, AdjustmentClaimStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentClaimStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentClaimStatus next)
    {
        switch (Status)
        {
            case AdjustmentClaimStatus.Planned:
                return next == AdjustmentClaimStatus.Scheduled || next == AdjustmentClaimStatus.InProgress;
            case AdjustmentClaimStatus.Scheduled:
                return next == AdjustmentClaimStatus.Completed;
            case AdjustmentClaimStatus.InProgress:
                switch (next)
                {
                    case AdjustmentClaimStatus.Completed:
                    case AdjustmentClaimStatus.Cancelled:
                        return true;
                }
                return false;
            case AdjustmentClaimStatus.Completed:
                return next == AdjustmentClaimStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AdjustmentClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public AdjustmentClaim Copy()
    {
        return new AdjustmentClaim(Code, Label, Locale, Skill, Unit, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
