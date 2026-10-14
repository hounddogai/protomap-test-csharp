namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition proposal is in its life cycle.</summary>
public enum RecognitionProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a recognition proposal and its current status.</summary>
public sealed class RecognitionProposal(string code, string name, int expiresAt, double cost, string grade, RecognitionProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Cost { get; set; } = cost;
    public string Grade { get; set; } = grade;
    public RecognitionProposalStatus Status { get; set; } = status;

    /// <summary>Starts a recognition proposal in status planned.</summary>
    public static RecognitionProposal NewRecognitionProposal(string code, string name)
    {
        return new RecognitionProposal(code, name, 0, 0.0, "", RecognitionProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionProposalStatus next)
    {
        switch (Status)
        {
            case RecognitionProposalStatus.Planned:
                switch (next)
                {
                    case RecognitionProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case RecognitionProposalStatus.Scheduled:
                return next == RecognitionProposalStatus.Cancelled;
            case RecognitionProposalStatus.InProgress:
                return next == RecognitionProposalStatus.Completed || next == RecognitionProposalStatus.Cancelled;
            case RecognitionProposalStatus.Completed:
                return next == RecognitionProposalStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RecognitionProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetExposure()
    {
        return Math.Round(Cost + (double)ExpiresAt, 2);
    }

    public RecognitionProposal Copy()
    {
        return new RecognitionProposal(Code, Name, ExpiresAt, Cost, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
