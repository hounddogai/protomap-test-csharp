namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback rate allocation is in its life cycle.</summary>
public enum FallbackRateAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A fallback rate allocation. Its code is unique within a tenant.</summary>
public sealed class FallbackRateAllocation(string code, string name, double ceiling, int expiresAt, string channel, FallbackRateAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Channel { get; set; } = channel;
    public FallbackRateAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a fallback rate allocation in status requested.</summary>
    public static FallbackRateAllocation NewFallbackRateAllocation(string code, string name)
    {
        return new FallbackRateAllocation(code, name, 0.0, 0, "", FallbackRateAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackRateAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackRateAllocationStatus next)
    {
        switch (Status)
        {
            case FallbackRateAllocationStatus.Requested:
                switch (next)
                {
                    case FallbackRateAllocationStatus.Granted:
                    case FallbackRateAllocationStatus.Revoked:
                        return true;
                }
                return false;
            case FallbackRateAllocationStatus.Granted:
                switch (next)
                {
                    case FallbackRateAllocationStatus.Revoked:
                    case FallbackRateAllocationStatus.Expired:
                        return true;
                }
                return false;
            case FallbackRateAllocationStatus.Revoked:
                return next == FallbackRateAllocationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(FallbackRateAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback rate allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"fallback rate allocation"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Ceiling - (double)ExpiresAt, 2);
    }

    public FallbackRateAllocation Copy()
    {
        return new FallbackRateAllocation(Code, Name, Ceiling, ExpiresAt, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
