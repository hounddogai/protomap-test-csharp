namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due proposal is in its life cycle.</summary>
public enum TrainingDueProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a training due proposal and its current status.</summary>
public sealed class TrainingDueProposal(string code, string name, Dictionary<string, string> attributes, string managerId, int limit, int quantity, double cost, double minimum, string skill, int priority, TrainingDueProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ManagerId { get; set; } = managerId;
    public int Limit { get; set; } = limit;
    public int Quantity { get; set; } = quantity;
    public double Cost { get; set; } = cost;
    public double Minimum { get; set; } = minimum;
    public string Skill { get; set; } = skill;
    public int Priority { get; set; } = priority;
    public TrainingDueProposalStatus Status { get; set; } = status;

    /// <summary>Starts a training due proposal in status draft.</summary>
    public static TrainingDueProposal NewTrainingDueProposal(string code, string name)
    {
        return new TrainingDueProposal(
            code,
            name,
            new Dictionary<string, string>(),
            "",
            0,
            0,
            0.0,
            0.0,
            "",
            0,
            TrainingDueProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueProposalStatus next)
    {
        switch (Status)
        {
            case TrainingDueProposalStatus.Draft:
                switch (next)
                {
                    case TrainingDueProposalStatus.Active:
                        return true;
                }
                return false;
            case TrainingDueProposalStatus.Pending:
                switch (next)
                {
                    case TrainingDueProposalStatus.Suspended:
                    case TrainingDueProposalStatus.Closed:
                        return true;
                }
                return false;
            case TrainingDueProposalStatus.Active:
                switch (next)
                {
                    case TrainingDueProposalStatus.Suspended:
                    case TrainingDueProposalStatus.Closed:
                        return true;
                }
                return false;
            case TrainingDueProposalStatus.Suspended:
                switch (next)
                {
                    case TrainingDueProposalStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double NetValue()
    {
        return Math.Round((double)Limit * (double)Priority, 2);
    }

    public TrainingDueProposal Copy()
    {
        return new TrainingDueProposal(
            Code,
            Name,
            Attributes,
            ManagerId,
            Limit,
            Quantity,
            Cost,
            Minimum,
            Skill,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
