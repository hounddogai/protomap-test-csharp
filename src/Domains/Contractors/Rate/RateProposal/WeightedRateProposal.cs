namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted rate proposal is in its life cycle.</summary>
public enum WeightedRateProposalStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A weighted rate proposal as the rate team keeps it.</summary>
public sealed class WeightedRateProposal(string code, string title, string badge, string source, string managerId, string category, string jobCode, bool archived, int quantity, WeightedRateProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public string Source { get; set; } = source;
    public string ManagerId { get; set; } = managerId;
    public string Category { get; set; } = category;
    public string JobCode { get; set; } = jobCode;
    public bool Archived { get; set; } = archived;
    public int Quantity { get; set; } = quantity;
    public WeightedRateProposalStatus Status { get; set; } = status;

    /// <summary>Starts a weighted rate proposal in status new.</summary>
    public static WeightedRateProposal NewWeightedRateProposal(string code, string title)
    {
        return new WeightedRateProposal(code, title, "", "", "", "", "", false, 0, WeightedRateProposalStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedRateProposalStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedRateProposalStatus next)
    {
        switch (Status)
        {
            case WeightedRateProposalStatus.New:
                switch (next)
                {
                    case WeightedRateProposalStatus.Queued:
                        return true;
                }
                return false;
            case WeightedRateProposalStatus.Queued:
                return next == WeightedRateProposalStatus.Running;
            case WeightedRateProposalStatus.Running:
                switch (next)
                {
                    case WeightedRateProposalStatus.Failed:
                        return true;
                }
                return false;
            case WeightedRateProposalStatus.Done:
                return next == WeightedRateProposalStatus.Failed;
        }
        return false;
    }

    public void MoveTo(WeightedRateProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted rate proposal {Code} cannot move from {Status} to {next}");
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

    public WeightedRateProposal Copy()
    {
        return new WeightedRateProposal(
            Code,
            Title,
            Badge,
            Source,
            ManagerId,
            Category,
            JobCode,
            Archived,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
