namespace Acme.Workforce.Engagement.Recognition.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition claim is in its life cycle.</summary>
public enum RecognitionClaimStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a recognition claim and its current status.</summary>
public sealed class RecognitionClaim(string code, string name, string contactEmail, double margin, double score, string channel, double floorValue, string title, string reviewer, string salaryBand, RecognitionClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public double Margin { get; set; } = margin;
    public double Score { get; set; } = score;
    public string Channel { get; set; } = channel;
    public double FloorValue { get; set; } = floorValue;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public string SalaryBand { get; set; } = salaryBand;
    public RecognitionClaimStatus Status { get; set; } = status;

    /// <summary>Starts a recognition claim in status idle.</summary>
    public static RecognitionClaim NewRecognitionClaim(string code, string name)
    {
        return new RecognitionClaim(code, name, "", 0.0, 0.0, "", 0.0, "", "", "", RecognitionClaimStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionClaimStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionClaimStatus next)
    {
        switch (Status)
        {
            case RecognitionClaimStatus.Idle:
                switch (next)
                {
                    case RecognitionClaimStatus.Ready:
                    case RecognitionClaimStatus.Draining:
                        return true;
                }
                return false;
            case RecognitionClaimStatus.Warming:
                switch (next)
                {
                    case RecognitionClaimStatus.Ready:
                    case RecognitionClaimStatus.Draining:
                        return true;
                }
                return false;
            case RecognitionClaimStatus.Ready:
                switch (next)
                {
                    case RecognitionClaimStatus.Draining:
                    case RecognitionClaimStatus.Stopped:
                        return true;
                }
                return false;
            case RecognitionClaimStatus.Draining:
                return next == RecognitionClaimStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RecognitionClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(Score - Margin, 2);
    }

    public RecognitionClaim Copy()
    {
        return new RecognitionClaim(
            Code,
            Name,
            ContactEmail,
            Margin,
            Score,
            Channel,
            FloorValue,
            Title,
            Reviewer,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
