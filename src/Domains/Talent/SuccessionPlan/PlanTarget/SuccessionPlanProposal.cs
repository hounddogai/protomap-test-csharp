namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan proposal is in its life cycle.</summary>
public enum SuccessionPlanProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A succession plan proposal. Its code is unique within a tenant.</summary>
public sealed class SuccessionPlanProposal(string code, string name, int startsAt, string managerId, string notes, int capacity, string label, double floorValue, double margin, int sequence, double hours, SuccessionPlanProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public string ManagerId { get; set; } = managerId;
    public string Notes { get; set; } = notes;
    public int Capacity { get; set; } = capacity;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public double Margin { get; set; } = margin;
    public int Sequence { get; set; } = sequence;
    public double Hours { get; set; } = hours;
    public SuccessionPlanProposalStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan proposal in status draft.</summary>
    public static SuccessionPlanProposal NewSuccessionPlanProposal(string code, string name)
    {
        return new SuccessionPlanProposal(
            code,
            name,
            0,
            "",
            "",
            0,
            "",
            0.0,
            0.0,
            0,
            0.0,
            SuccessionPlanProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanProposalStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanProposalStatus.Draft:
                return next == SuccessionPlanProposalStatus.Pending;
            case SuccessionPlanProposalStatus.Pending:
                return next == SuccessionPlanProposalStatus.Closed;
            case SuccessionPlanProposalStatus.Active:
                return next == SuccessionPlanProposalStatus.Suspended || next == SuccessionPlanProposalStatus.Closed;
            case SuccessionPlanProposalStatus.Suspended:
                return next == SuccessionPlanProposalStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"succession plan proposal"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Margin - FloorValue, 2);
    }

    public SuccessionPlanProposal Copy()
    {
        return new SuccessionPlanProposal(
            Code,
            Name,
            StartsAt,
            ManagerId,
            Notes,
            Capacity,
            Label,
            FloorValue,
            Margin,
            Sequence,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
