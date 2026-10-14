namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request proposal is in its life cycle.</summary>
public enum LeaveRequestProposalStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A leave request proposal. Its code is unique within a tenant.</summary>
public sealed class LeaveRequestProposal(string code, string label, int attempts, string department, Dictionary<string, string> attributes, string phone, int expiresAt, string externalRef, LeaveRequestProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public string Department { get; set; } = department;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Phone { get; set; } = phone;
    public int ExpiresAt { get; set; } = expiresAt;
    public string ExternalRef { get; set; } = externalRef;
    public LeaveRequestProposalStatus Status { get; set; } = status;

    /// <summary>Starts a leave request proposal in status created.</summary>
    public static LeaveRequestProposal NewLeaveRequestProposal(string code, string label)
    {
        return new LeaveRequestProposal(
            code,
            label,
            0,
            "",
            new Dictionary<string, string>(),
            "",
            0,
            "",
            LeaveRequestProposalStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestProposalStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestProposalStatus next)
    {
        switch (Status)
        {
            case LeaveRequestProposalStatus.Created:
                return next == LeaveRequestProposalStatus.Retired;
            case LeaveRequestProposalStatus.Validated:
                switch (next)
                {
                    case LeaveRequestProposalStatus.Retired:
                        return true;
                }
                return false;
            case LeaveRequestProposalStatus.Published:
                switch (next)
                {
                    case LeaveRequestProposalStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)ExpiresAt * (double)Attempts, 2);
    }

    public LeaveRequestProposal Copy()
    {
        return new LeaveRequestProposal(
            Code,
            Label,
            Attempts,
            Department,
            Attributes,
            Phone,
            ExpiresAt,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
