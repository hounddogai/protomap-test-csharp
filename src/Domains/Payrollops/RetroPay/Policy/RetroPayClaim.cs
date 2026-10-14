namespace Acme.Workforce.Payrollops.RetroPay.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay claim is in its life cycle.</summary>
public enum RetroPayClaimStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a retro pay claim and its current status.</summary>
public sealed class RetroPayClaim(string code, string name, string source, string checksum, string department, double ratio, double ceiling, double rate, RetroPayClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public string Checksum { get; set; } = checksum;
    public string Department { get; set; } = department;
    public double Ratio { get; set; } = ratio;
    public double Ceiling { get; set; } = ceiling;
    public double Rate { get; set; } = rate;
    public RetroPayClaimStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay claim in status planned.</summary>
    public static RetroPayClaim NewRetroPayClaim(string code, string name)
    {
        return new RetroPayClaim(code, name, "", "", "", 0.0, 0.0, 0.0, RetroPayClaimStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayClaimStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayClaimStatus next)
    {
        switch (Status)
        {
            case RetroPayClaimStatus.Planned:
                switch (next)
                {
                    case RetroPayClaimStatus.Scheduled:
                    case RetroPayClaimStatus.InProgress:
                        return true;
                }
                return false;
            case RetroPayClaimStatus.Scheduled:
                return next == RetroPayClaimStatus.InProgress || next == RetroPayClaimStatus.Cancelled;
            case RetroPayClaimStatus.InProgress:
                return next == RetroPayClaimStatus.Completed || next == RetroPayClaimStatus.Cancelled;
            case RetroPayClaimStatus.Completed:
                switch (next)
                {
                    case RetroPayClaimStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"retro pay claim"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Ratio + Ceiling, 2);
    }

    public RetroPayClaim Copy()
    {
        return new RetroPayClaim(Code, Name, Source, Checksum, Department, Ratio, Ceiling, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
