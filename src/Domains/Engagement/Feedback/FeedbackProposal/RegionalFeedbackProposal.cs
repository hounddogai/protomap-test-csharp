namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional feedback proposal is in its life cycle.</summary>
public enum RegionalFeedbackProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A regional feedback proposal. Its code is unique within a tenant.</summary>
public sealed class RegionalFeedbackProposal(string code, string label, double hours, string employeeId, int updatedAt, int limit, string channel, int retries, string jobCode, RegionalFeedbackProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public string EmployeeId { get; set; } = employeeId;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Limit { get; set; } = limit;
    public string Channel { get; set; } = channel;
    public int Retries { get; set; } = retries;
    public string JobCode { get; set; } = jobCode;
    public RegionalFeedbackProposalStatus Status { get; set; } = status;

    /// <summary>Starts a regional feedback proposal in status planned.</summary>
    public static RegionalFeedbackProposal NewRegionalFeedbackProposal(string code, string label)
    {
        return new RegionalFeedbackProposal(
            code,
            label,
            0.0,
            "",
            0,
            0,
            "",
            0,
            "",
            RegionalFeedbackProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalFeedbackProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalFeedbackProposalStatus next)
    {
        switch (Status)
        {
            case RegionalFeedbackProposalStatus.Planned:
                return next == RegionalFeedbackProposalStatus.Scheduled || next == RegionalFeedbackProposalStatus.Completed;
            case RegionalFeedbackProposalStatus.Scheduled:
                return next == RegionalFeedbackProposalStatus.Cancelled;
            case RegionalFeedbackProposalStatus.InProgress:
                switch (next)
                {
                    case RegionalFeedbackProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case RegionalFeedbackProposalStatus.Completed:
                switch (next)
                {
                    case RegionalFeedbackProposalStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalFeedbackProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional feedback proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)UpdatedAt * (double)Retries, 2);
    }

    public RegionalFeedbackProposal Copy()
    {
        return new RegionalFeedbackProposal(
            Code,
            Label,
            Hours,
            EmployeeId,
            UpdatedAt,
            Limit,
            Channel,
            Retries,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
