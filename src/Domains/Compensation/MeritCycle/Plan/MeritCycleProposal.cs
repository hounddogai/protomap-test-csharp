namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle proposal is in its life cycle.</summary>
public enum MeritCycleProposalStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One merit cycle proposal, with what the planners need to act on it.</summary>
public sealed class MeritCycleProposal(string code, string name, string summary, int windowDays, string employeeId, int tenureMonths, bool archived, string source, string title, MeritCycleProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public int WindowDays { get; set; } = windowDays;
    public string EmployeeId { get; set; } = employeeId;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Archived { get; set; } = archived;
    public string Source { get; set; } = source;
    public string Title { get; set; } = title;
    public MeritCycleProposalStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle proposal in status new.</summary>
    public static MeritCycleProposal NewMeritCycleProposal(string code, string name)
    {
        return new MeritCycleProposal(code, name, "", 0, "", 0, false, "", "", MeritCycleProposalStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleProposalStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleProposalStatus next)
    {
        switch (Status)
        {
            case MeritCycleProposalStatus.New:
                switch (next)
                {
                    case MeritCycleProposalStatus.Queued:
                    case MeritCycleProposalStatus.Failed:
                        return true;
                }
                return false;
            case MeritCycleProposalStatus.Queued:
                return next == MeritCycleProposalStatus.Running;
            case MeritCycleProposalStatus.Running:
                switch (next)
                {
                    case MeritCycleProposalStatus.Done:
                        return true;
                }
                return false;
            case MeritCycleProposalStatus.Done:
                return next == MeritCycleProposalStatus.Failed;
        }
        return false;
    }

    public void MoveTo(MeritCycleProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)WindowDays - (double)TenureMonths, 2);
    }

    public MeritCycleProposal Copy()
    {
        return new MeritCycleProposal(
            Code,
            Name,
            Summary,
            WindowDays,
            EmployeeId,
            TenureMonths,
            Archived,
            Source,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
