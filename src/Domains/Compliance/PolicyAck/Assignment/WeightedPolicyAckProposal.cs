namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted policy ack proposal is in its life cycle.</summary>
public enum WeightedPolicyAckProposalStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted policy ack proposal as the policy ack team keeps it.</summary>
public sealed class WeightedPolicyAckProposal(string code, string title, string badge, int sequence, double maximum, string currency, int windowDays, int graceDays, double rate, double rating, WeightedPolicyAckProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public int Sequence { get; set; } = sequence;
    public double Maximum { get; set; } = maximum;
    public string Currency { get; set; } = currency;
    public int WindowDays { get; set; } = windowDays;
    public int GraceDays { get; set; } = graceDays;
    public double Rate { get; set; } = rate;
    public double Rating { get; set; } = rating;
    public WeightedPolicyAckProposalStatus Status { get; set; } = status;

    /// <summary>Starts a weighted policy ack proposal in status proposed.</summary>
    public static WeightedPolicyAckProposal NewWeightedPolicyAckProposal(string code, string title)
    {
        return new WeightedPolicyAckProposal(
            code,
            title,
            "",
            0,
            0.0,
            "",
            0,
            0,
            0.0,
            0.0,
            WeightedPolicyAckProposalStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedPolicyAckProposalStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedPolicyAckProposalStatus next)
    {
        switch (Status)
        {
            case WeightedPolicyAckProposalStatus.Proposed:
                return next == WeightedPolicyAckProposalStatus.Withdrawn;
            case WeightedPolicyAckProposalStatus.Approved:
                switch (next)
                {
                    case WeightedPolicyAckProposalStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedPolicyAckProposalStatus.Rejected:
                return next == WeightedPolicyAckProposalStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedPolicyAckProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted policy ack proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted policy ack proposal"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)GraceDays * (double)WindowDays, 2);
    }

    public WeightedPolicyAckProposal Copy()
    {
        return new WeightedPolicyAckProposal(
            Code,
            Title,
            Badge,
            Sequence,
            Maximum,
            Currency,
            WindowDays,
            GraceDays,
            Rate,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
