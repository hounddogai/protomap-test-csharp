namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan proposal is in its life cycle.</summary>
public enum ReturnPlanProposalStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One return plan proposal, with what the planners need to act on it.</summary>
public sealed class ReturnPlanProposal(string code, string label, string team, double amount, string locale, double discount, int expiresAt, ReturnPlanProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public double Amount { get; set; } = amount;
    public string Locale { get; set; } = locale;
    public double Discount { get; set; } = discount;
    public int ExpiresAt { get; set; } = expiresAt;
    public ReturnPlanProposalStatus Status { get; set; } = status;

    /// <summary>Starts a return plan proposal in status proposed.</summary>
    public static ReturnPlanProposal NewReturnPlanProposal(string code, string label)
    {
        return new ReturnPlanProposal(code, label, "", 0.0, "", 0.0, 0, ReturnPlanProposalStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanProposalStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanProposalStatus next)
    {
        switch (Status)
        {
            case ReturnPlanProposalStatus.Proposed:
                switch (next)
                {
                    case ReturnPlanProposalStatus.Approved:
                        return true;
                }
                return false;
            case ReturnPlanProposalStatus.Approved:
                return next == ReturnPlanProposalStatus.Rejected || next == ReturnPlanProposalStatus.Withdrawn;
            case ReturnPlanProposalStatus.Rejected:
                return next == ReturnPlanProposalStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ReturnPlanProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"return plan proposal"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)ExpiresAt * Amount, 2);
    }

    public ReturnPlanProposal Copy()
    {
        return new ReturnPlanProposal(Code, Label, Team, Amount, Locale, Discount, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
