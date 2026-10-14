namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility proposal is in its life cycle.</summary>
public enum MobilityProposalStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a mobility proposal and its current status.</summary>
public sealed class MobilityProposal(string code, string name, double score, string title, double ratio, string source, MobilityProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Source { get; set; } = source;
    public MobilityProposalStatus Status { get; set; } = status;

    /// <summary>Starts a mobility proposal in status new.</summary>
    public static MobilityProposal NewMobilityProposal(string code, string name)
    {
        return new MobilityProposal(code, name, 0.0, "", 0.0, "", MobilityProposalStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityProposalStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityProposalStatus next)
    {
        switch (Status)
        {
            case MobilityProposalStatus.New:
                return next == MobilityProposalStatus.Running;
            case MobilityProposalStatus.Queued:
                switch (next)
                {
                    case MobilityProposalStatus.Running:
                    case MobilityProposalStatus.Done:
                        return true;
                }
                return false;
            case MobilityProposalStatus.Running:
                return next == MobilityProposalStatus.Done || next == MobilityProposalStatus.Failed;
            case MobilityProposalStatus.Done:
                switch (next)
                {
                    case MobilityProposalStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Score + Ratio, 2);
    }

    public MobilityProposal Copy()
    {
        return new MobilityProposal(Code, Name, Score, Title, Ratio, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
