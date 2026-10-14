namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling bonus pool proposal is in its life cycle.</summary>
public enum RollingBonusPoolProposalStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A rolling bonus pool proposal as the bonus pool team keeps it.</summary>
public sealed class RollingBonusPoolProposal(string code, string title, string reviewer, string channel, string timezone, string externalRef, List<string> tags, double amount, RollingBonusPoolProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public string Channel { get; set; } = channel;
    public string Timezone { get; set; } = timezone;
    public string ExternalRef { get; set; } = externalRef;
    public List<string> Tags { get; set; } = tags;
    public double Amount { get; set; } = amount;
    public RollingBonusPoolProposalStatus Status { get; set; } = status;

    /// <summary>Starts a rolling bonus pool proposal in status new.</summary>
    public static RollingBonusPoolProposal NewRollingBonusPoolProposal(string code, string title)
    {
        return new RollingBonusPoolProposal(
            code,
            title,
            "",
            "",
            "",
            "",
            new List<string>(),
            0.0,
            RollingBonusPoolProposalStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingBonusPoolProposalStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingBonusPoolProposalStatus next)
    {
        switch (Status)
        {
            case RollingBonusPoolProposalStatus.New:
                switch (next)
                {
                    case RollingBonusPoolProposalStatus.Running:
                        return true;
                }
                return false;
            case RollingBonusPoolProposalStatus.Queued:
                return next == RollingBonusPoolProposalStatus.Failed;
            case RollingBonusPoolProposalStatus.Running:
                switch (next)
                {
                    case RollingBonusPoolProposalStatus.Done:
                    case RollingBonusPoolProposalStatus.Failed:
                        return true;
                }
                return false;
            case RollingBonusPoolProposalStatus.Done:
                return next == RollingBonusPoolProposalStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RollingBonusPoolProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling bonus pool proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public RollingBonusPoolProposal Copy()
    {
        return new RollingBonusPoolProposal(
            Code,
            Title,
            Reviewer,
            Channel,
            Timezone,
            ExternalRef,
            Tags,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
