namespace Acme.Workforce.Engagement.Feedback.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling feedback proposal is in its life cycle.</summary>
public enum RollingFeedbackProposalStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A rolling feedback proposal. Its code is unique within a tenant.</summary>
public sealed class RollingFeedbackProposal(string code, string label, string externalRef, int leaveDays, int batchSize, double rating, double ratio, double score, double margin, string channel, string source, RollingFeedbackProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public int LeaveDays { get; set; } = leaveDays;
    public int BatchSize { get; set; } = batchSize;
    public double Rating { get; set; } = rating;
    public double Ratio { get; set; } = ratio;
    public double Score { get; set; } = score;
    public double Margin { get; set; } = margin;
    public string Channel { get; set; } = channel;
    public string Source { get; set; } = source;
    public RollingFeedbackProposalStatus Status { get; set; } = status;

    /// <summary>Starts a rolling feedback proposal in status created.</summary>
    public static RollingFeedbackProposal NewRollingFeedbackProposal(string code, string label)
    {
        return new RollingFeedbackProposal(
            code,
            label,
            "",
            0,
            0,
            0.0,
            0.0,
            0.0,
            0.0,
            "",
            "",
            RollingFeedbackProposalStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingFeedbackProposalStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingFeedbackProposalStatus next)
    {
        switch (Status)
        {
            case RollingFeedbackProposalStatus.Created:
                switch (next)
                {
                    case RollingFeedbackProposalStatus.Published:
                    case RollingFeedbackProposalStatus.Retired:
                        return true;
                }
                return false;
            case RollingFeedbackProposalStatus.Validated:
                switch (next)
                {
                    case RollingFeedbackProposalStatus.Published:
                        return true;
                }
                return false;
            case RollingFeedbackProposalStatus.Published:
                return next == RollingFeedbackProposalStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RollingFeedbackProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling feedback proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetValue()
    {
        return Math.Round(Ratio + (double)BatchSize, 2);
    }

    public RollingFeedbackProposal Copy()
    {
        return new RollingFeedbackProposal(
            Code,
            Label,
            ExternalRef,
            LeaveDays,
            BatchSize,
            Rating,
            Ratio,
            Score,
            Margin,
            Channel,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
