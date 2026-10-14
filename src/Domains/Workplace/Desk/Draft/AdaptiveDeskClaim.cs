namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive desk claim is in its life cycle.</summary>
public enum AdaptiveDeskClaimStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a adaptive desk claim and its current status.</summary>
public sealed class AdaptiveDeskClaim(string code, string label, int attempts, string employeeId, int createdAt, double threshold, string channel, AdaptiveDeskClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public string EmployeeId { get; set; } = employeeId;
    public int CreatedAt { get; set; } = createdAt;
    public double Threshold { get; set; } = threshold;
    public string Channel { get; set; } = channel;
    public AdaptiveDeskClaimStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive desk claim in status idle.</summary>
    public static AdaptiveDeskClaim NewAdaptiveDeskClaim(string code, string label)
    {
        return new AdaptiveDeskClaim(code, label, 0, "", 0, 0.0, "", AdaptiveDeskClaimStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveDeskClaimStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveDeskClaimStatus next)
    {
        switch (Status)
        {
            case AdaptiveDeskClaimStatus.Idle:
                return next == AdaptiveDeskClaimStatus.Ready;
            case AdaptiveDeskClaimStatus.Warming:
                switch (next)
                {
                    case AdaptiveDeskClaimStatus.Draining:
                        return true;
                }
                return false;
            case AdaptiveDeskClaimStatus.Ready:
                switch (next)
                {
                    case AdaptiveDeskClaimStatus.Draining:
                    case AdaptiveDeskClaimStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveDeskClaimStatus.Draining:
                switch (next)
                {
                    case AdaptiveDeskClaimStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveDeskClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive desk claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Attempts + (double)CreatedAt, 2);
    }

    public AdaptiveDeskClaim Copy()
    {
        return new AdaptiveDeskClaim(Code, Label, Attempts, EmployeeId, CreatedAt, Threshold, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
