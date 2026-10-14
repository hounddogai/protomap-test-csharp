namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped retro pay proposal is in its life cycle.</summary>
public enum CappedRetroPayProposalStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A capped retro pay proposal as the retro pay team keeps it.</summary>
public sealed class CappedRetroPayProposal(string code, string title, double maximum, int durationMinutes, double budget, string source, int revision, double weight, string externalRef, string checksum, CappedRetroPayProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Budget { get; set; } = budget;
    public string Source { get; set; } = source;
    public int Revision { get; set; } = revision;
    public double Weight { get; set; } = weight;
    public string ExternalRef { get; set; } = externalRef;
    public string Checksum { get; set; } = checksum;
    public CappedRetroPayProposalStatus Status { get; set; } = status;

    /// <summary>Starts a capped retro pay proposal in status open.</summary>
    public static CappedRetroPayProposal NewCappedRetroPayProposal(string code, string title)
    {
        return new CappedRetroPayProposal(
            code,
            title,
            0.0,
            0,
            0.0,
            "",
            0,
            0.0,
            "",
            "",
            CappedRetroPayProposalStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedRetroPayProposalStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedRetroPayProposalStatus next)
    {
        switch (Status)
        {
            case CappedRetroPayProposalStatus.Open:
                switch (next)
                {
                    case CappedRetroPayProposalStatus.Archived:
                        return true;
                }
                return false;
            case CappedRetroPayProposalStatus.InReview:
                return next == CappedRetroPayProposalStatus.Resolved;
            case CappedRetroPayProposalStatus.Resolved:
                return next == CappedRetroPayProposalStatus.Reopened || next == CappedRetroPayProposalStatus.Archived;
            case CappedRetroPayProposalStatus.Reopened:
                return next == CappedRetroPayProposalStatus.Archived;
        }
        return false;
    }

    public void MoveTo(CappedRetroPayProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped retro pay proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedValue()
    {
        return Math.Round(Budget + Weight, 2);
    }

    public CappedRetroPayProposal Copy()
    {
        return new CappedRetroPayProposal(
            Code,
            Title,
            Maximum,
            DurationMinutes,
            Budget,
            Source,
            Revision,
            Weight,
            ExternalRef,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
