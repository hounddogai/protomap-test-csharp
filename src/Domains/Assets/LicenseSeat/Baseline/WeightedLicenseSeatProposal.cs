namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted license seat proposal is in its life cycle.</summary>
public enum WeightedLicenseSeatProposalStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A weighted license seat proposal. Its code is unique within a tenant.</summary>
public sealed class WeightedLicenseSeatProposal(string code, string name, string channel, string unit, int retries, string costCenter, WeightedLicenseSeatProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public string Unit { get; set; } = unit;
    public int Retries { get; set; } = retries;
    public string CostCenter { get; set; } = costCenter;
    public WeightedLicenseSeatProposalStatus Status { get; set; } = status;

    /// <summary>Starts a weighted license seat proposal in status open.</summary>
    public static WeightedLicenseSeatProposal NewWeightedLicenseSeatProposal(string code, string name)
    {
        return new WeightedLicenseSeatProposal(code, name, "", "", 0, "", WeightedLicenseSeatProposalStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedLicenseSeatProposalStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedLicenseSeatProposalStatus next)
    {
        switch (Status)
        {
            case WeightedLicenseSeatProposalStatus.Open:
                switch (next)
                {
                    case WeightedLicenseSeatProposalStatus.InReview:
                        return true;
                }
                return false;
            case WeightedLicenseSeatProposalStatus.InReview:
                return next == WeightedLicenseSeatProposalStatus.Resolved || next == WeightedLicenseSeatProposalStatus.Archived;
            case WeightedLicenseSeatProposalStatus.Resolved:
                switch (next)
                {
                    case WeightedLicenseSeatProposalStatus.Reopened:
                    case WeightedLicenseSeatProposalStatus.Archived:
                        return true;
                }
                return false;
            case WeightedLicenseSeatProposalStatus.Reopened:
                return next == WeightedLicenseSeatProposalStatus.Archived;
        }
        return false;
    }

    public void MoveTo(WeightedLicenseSeatProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted license seat proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public WeightedLicenseSeatProposal Copy()
    {
        return new WeightedLicenseSeatProposal(Code, Name, Channel, Unit, Retries, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
