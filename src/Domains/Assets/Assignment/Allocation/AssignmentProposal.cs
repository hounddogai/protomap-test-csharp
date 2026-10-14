namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment proposal is in its life cycle.</summary>
public enum AssignmentProposalStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A assignment proposal as the assignment team keeps it.</summary>
public sealed class AssignmentProposal(string code, string title, string employeeId, double amount, int tenureMonths, bool pinned, bool visible, int expiresAt, string reviewer, string managerId, AssignmentProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public double Amount { get; set; } = amount;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Pinned { get; set; } = pinned;
    public bool Visible { get; set; } = visible;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Reviewer { get; set; } = reviewer;
    public string ManagerId { get; set; } = managerId;
    public AssignmentProposalStatus Status { get; set; } = status;

    /// <summary>Starts a assignment proposal in status idle.</summary>
    public static AssignmentProposal NewAssignmentProposal(string code, string title)
    {
        return new AssignmentProposal(
            code,
            title,
            "",
            0.0,
            0,
            false,
            false,
            0,
            "",
            "",
            AssignmentProposalStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentProposalStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentProposalStatus next)
    {
        switch (Status)
        {
            case AssignmentProposalStatus.Idle:
                switch (next)
                {
                    case AssignmentProposalStatus.Ready:
                        return true;
                }
                return false;
            case AssignmentProposalStatus.Warming:
                return next == AssignmentProposalStatus.Draining || next == AssignmentProposalStatus.Stopped;
            case AssignmentProposalStatus.Ready:
                switch (next)
                {
                    case AssignmentProposalStatus.Draining:
                    case AssignmentProposalStatus.Stopped:
                        return true;
                }
                return false;
            case AssignmentProposalStatus.Draining:
                switch (next)
                {
                    case AssignmentProposalStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetValue()
    {
        return Math.Round(Amount * (double)TenureMonths, 2);
    }

    public AssignmentProposal Copy()
    {
        return new AssignmentProposal(
            Code,
            Title,
            EmployeeId,
            Amount,
            TenureMonths,
            Pinned,
            Visible,
            ExpiresAt,
            Reviewer,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
