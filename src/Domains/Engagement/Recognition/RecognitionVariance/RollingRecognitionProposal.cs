namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling recognition proposal is in its life cycle.</summary>
public enum RollingRecognitionProposalStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A rolling recognition proposal as the recognition team keeps it.</summary>
public sealed class RollingRecognitionProposal(string code, string label, int quantity, int limit, double fte, int capacity, string country, RollingRecognitionProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public int Limit { get; set; } = limit;
    public double Fte { get; set; } = fte;
    public int Capacity { get; set; } = capacity;
    public string Country { get; set; } = country;
    public RollingRecognitionProposalStatus Status { get; set; } = status;

    /// <summary>Starts a rolling recognition proposal in status new.</summary>
    public static RollingRecognitionProposal NewRollingRecognitionProposal(string code, string label)
    {
        return new RollingRecognitionProposal(code, label, 0, 0, 0.0, 0, "", RollingRecognitionProposalStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRecognitionProposalStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRecognitionProposalStatus next)
    {
        switch (Status)
        {
            case RollingRecognitionProposalStatus.New:
                return next == RollingRecognitionProposalStatus.Running;
            case RollingRecognitionProposalStatus.Queued:
                return next == RollingRecognitionProposalStatus.Running || next == RollingRecognitionProposalStatus.Done;
            case RollingRecognitionProposalStatus.Running:
                return next == RollingRecognitionProposalStatus.Done || next == RollingRecognitionProposalStatus.Failed;
            case RollingRecognitionProposalStatus.Done:
                return next == RollingRecognitionProposalStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RollingRecognitionProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling recognition proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling recognition proposal"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Limit + Fte, 2);
    }

    public RollingRecognitionProposal Copy()
    {
        return new RollingRecognitionProposal(Code, Label, Quantity, Limit, Fte, Capacity, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
