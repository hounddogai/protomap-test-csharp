namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback balance claim is in its life cycle.</summary>
public enum FallbackBalanceClaimStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One fallback balance claim, with what the planners need to act on it.</summary>
public sealed class FallbackBalanceClaim(string code, string label, string unit, string team, double budget, bool approved, FallbackBalanceClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public string Team { get; set; } = team;
    public double Budget { get; set; } = budget;
    public bool Approved { get; set; } = approved;
    public FallbackBalanceClaimStatus Status { get; set; } = status;

    /// <summary>Starts a fallback balance claim in status planned.</summary>
    public static FallbackBalanceClaim NewFallbackBalanceClaim(string code, string label)
    {
        return new FallbackBalanceClaim(code, label, "", "", 0.0, false, FallbackBalanceClaimStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackBalanceClaimStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackBalanceClaimStatus next)
    {
        switch (Status)
        {
            case FallbackBalanceClaimStatus.Planned:
                switch (next)
                {
                    case FallbackBalanceClaimStatus.Scheduled:
                        return true;
                }
                return false;
            case FallbackBalanceClaimStatus.Scheduled:
                switch (next)
                {
                    case FallbackBalanceClaimStatus.Completed:
                    case FallbackBalanceClaimStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackBalanceClaimStatus.InProgress:
                return next == FallbackBalanceClaimStatus.Cancelled;
            case FallbackBalanceClaimStatus.Completed:
                switch (next)
                {
                    case FallbackBalanceClaimStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackBalanceClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback balance claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public FallbackBalanceClaim Copy()
    {
        return new FallbackBalanceClaim(Code, Label, Unit, Team, Budget, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
