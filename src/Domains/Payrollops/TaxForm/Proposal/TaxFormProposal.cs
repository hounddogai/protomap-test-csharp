namespace Acme.Workforce.Payrollops.TaxForm.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form proposal is in its life cycle.</summary>
public enum TaxFormProposalStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a tax form proposal and its current status.</summary>
public sealed class TaxFormProposal(string code, string label, bool active, bool approved, bool visible, double score, TaxFormProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public bool Approved { get; set; } = approved;
    public bool Visible { get; set; } = visible;
    public double Score { get; set; } = score;
    public TaxFormProposalStatus Status { get; set; } = status;

    /// <summary>Starts a tax form proposal in status idle.</summary>
    public static TaxFormProposal NewTaxFormProposal(string code, string label)
    {
        return new TaxFormProposal(code, label, false, false, false, 0.0, TaxFormProposalStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormProposalStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormProposalStatus next)
    {
        switch (Status)
        {
            case TaxFormProposalStatus.Idle:
                switch (next)
                {
                    case TaxFormProposalStatus.Ready:
                    case TaxFormProposalStatus.Draining:
                        return true;
                }
                return false;
            case TaxFormProposalStatus.Warming:
                switch (next)
                {
                    case TaxFormProposalStatus.Ready:
                    case TaxFormProposalStatus.Draining:
                        return true;
                }
                return false;
            case TaxFormProposalStatus.Ready:
                return next == TaxFormProposalStatus.Draining || next == TaxFormProposalStatus.Stopped;
            case TaxFormProposalStatus.Draining:
                switch (next)
                {
                    case TaxFormProposalStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"tax form proposal"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public TaxFormProposal Copy()
    {
        return new TaxFormProposal(Code, Label, Active, Approved, Visible, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
