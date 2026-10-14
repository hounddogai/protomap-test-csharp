namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy proposal is in its life cycle.</summary>
public enum PolicyProposalStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a policy proposal and its current status.</summary>
public sealed class PolicyProposal(string code, string name, bool visible, int batchSize, string ownerId, string employeeId, double margin, PolicyProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public int BatchSize { get; set; } = batchSize;
    public string OwnerId { get; set; } = ownerId;
    public string EmployeeId { get; set; } = employeeId;
    public double Margin { get; set; } = margin;
    public PolicyProposalStatus Status { get; set; } = status;

    /// <summary>Starts a policy proposal in status new.</summary>
    public static PolicyProposal NewPolicyProposal(string code, string name)
    {
        return new PolicyProposal(code, name, false, 0, "", "", 0.0, PolicyProposalStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyProposalStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyProposalStatus next)
    {
        switch (Status)
        {
            case PolicyProposalStatus.New:
                switch (next)
                {
                    case PolicyProposalStatus.Running:
                        return true;
                }
                return false;
            case PolicyProposalStatus.Queued:
                return next == PolicyProposalStatus.Failed;
            case PolicyProposalStatus.Running:
                switch (next)
                {
                    case PolicyProposalStatus.Done:
                    case PolicyProposalStatus.Failed:
                        return true;
                }
                return false;
            case PolicyProposalStatus.Done:
                switch (next)
                {
                    case PolicyProposalStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)BatchSize * Margin, 2);
    }

    public PolicyProposal Copy()
    {
        return new PolicyProposal(Code, Name, Visible, BatchSize, OwnerId, EmployeeId, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
