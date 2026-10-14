namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled benchmark proposal is in its life cycle.</summary>
public enum ScheduledBenchmarkProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a scheduled benchmark proposal and its current status.</summary>
public sealed class ScheduledBenchmarkProposal(string code, string title, double maximum, double threshold, int revision, string region, ScheduledBenchmarkProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public double Threshold { get; set; } = threshold;
    public int Revision { get; set; } = revision;
    public string Region { get; set; } = region;
    public ScheduledBenchmarkProposalStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled benchmark proposal in status draft.</summary>
    public static ScheduledBenchmarkProposal NewScheduledBenchmarkProposal(string code, string title)
    {
        return new ScheduledBenchmarkProposal(code, title, 0.0, 0.0, 0, "", ScheduledBenchmarkProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledBenchmarkProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledBenchmarkProposalStatus next)
    {
        switch (Status)
        {
            case ScheduledBenchmarkProposalStatus.Draft:
                return next == ScheduledBenchmarkProposalStatus.Pending || next == ScheduledBenchmarkProposalStatus.Suspended;
            case ScheduledBenchmarkProposalStatus.Pending:
                return next == ScheduledBenchmarkProposalStatus.Active;
            case ScheduledBenchmarkProposalStatus.Active:
                return next == ScheduledBenchmarkProposalStatus.Closed;
            case ScheduledBenchmarkProposalStatus.Suspended:
                return next == ScheduledBenchmarkProposalStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ScheduledBenchmarkProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled benchmark proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Maximum - Threshold, 2);
    }

    public ScheduledBenchmarkProposal Copy()
    {
        return new ScheduledBenchmarkProposal(Code, Title, Maximum, Threshold, Revision, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
