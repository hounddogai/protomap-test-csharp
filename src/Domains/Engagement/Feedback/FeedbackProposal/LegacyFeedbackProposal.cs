namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy feedback proposal is in its life cycle.</summary>
public enum LegacyFeedbackProposalStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a legacy feedback proposal and its current status.</summary>
public sealed class LegacyFeedbackProposal(string code, string label, int updatedAt, string notes, int priority, string skill, int capacity, LegacyFeedbackProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Notes { get; set; } = notes;
    public int Priority { get; set; } = priority;
    public string Skill { get; set; } = skill;
    public int Capacity { get; set; } = capacity;
    public LegacyFeedbackProposalStatus Status { get; set; } = status;

    /// <summary>Starts a legacy feedback proposal in status proposed.</summary>
    public static LegacyFeedbackProposal NewLegacyFeedbackProposal(string code, string label)
    {
        return new LegacyFeedbackProposal(code, label, 0, "", 0, "", 0, LegacyFeedbackProposalStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyFeedbackProposalStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyFeedbackProposalStatus next)
    {
        switch (Status)
        {
            case LegacyFeedbackProposalStatus.Proposed:
                switch (next)
                {
                    case LegacyFeedbackProposalStatus.Withdrawn:
                        return true;
                }
                return false;
            case LegacyFeedbackProposalStatus.Approved:
                switch (next)
                {
                    case LegacyFeedbackProposalStatus.Rejected:
                    case LegacyFeedbackProposalStatus.Withdrawn:
                        return true;
                }
                return false;
            case LegacyFeedbackProposalStatus.Rejected:
                switch (next)
                {
                    case LegacyFeedbackProposalStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyFeedbackProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy feedback proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Priority * (double)Capacity, 2);
    }

    public LegacyFeedbackProposal Copy()
    {
        return new LegacyFeedbackProposal(Code, Label, UpdatedAt, Notes, Priority, Skill, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
