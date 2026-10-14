namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay proposal is in its life cycle.</summary>
public enum RetroPayProposalStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a retro pay proposal and its current status.</summary>
public sealed class RetroPayProposal(string code, string name, string jobCode, double discount, double margin, string grade, string source, int updatedAt, string channel, int retries, RetroPayProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public double Discount { get; set; } = discount;
    public double Margin { get; set; } = margin;
    public string Grade { get; set; } = grade;
    public string Source { get; set; } = source;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Channel { get; set; } = channel;
    public int Retries { get; set; } = retries;
    public RetroPayProposalStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay proposal in status new.</summary>
    public static RetroPayProposal NewRetroPayProposal(string code, string name)
    {
        return new RetroPayProposal(code, name, "", 0.0, 0.0, "", "", 0, "", 0, RetroPayProposalStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayProposalStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayProposalStatus next)
    {
        switch (Status)
        {
            case RetroPayProposalStatus.New:
                return next == RetroPayProposalStatus.Done || next == RetroPayProposalStatus.Failed;
            case RetroPayProposalStatus.Queued:
                return next == RetroPayProposalStatus.Running || next == RetroPayProposalStatus.Done;
            case RetroPayProposalStatus.Running:
                return next == RetroPayProposalStatus.Done || next == RetroPayProposalStatus.Failed;
            case RetroPayProposalStatus.Done:
                return next == RetroPayProposalStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RetroPayProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"retro pay proposal"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)UpdatedAt + Margin, 2);
    }

    public RetroPayProposal Copy()
    {
        return new RetroPayProposal(
            Code,
            Name,
            JobCode,
            Discount,
            Margin,
            Grade,
            Source,
            UpdatedAt,
            Channel,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
