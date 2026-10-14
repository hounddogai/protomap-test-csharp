namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback offboarding allocation is in its life cycle.</summary>
public enum FallbackOffboardingAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a fallback offboarding allocation and its current status.</summary>
public sealed class FallbackOffboardingAllocation(string code, string name, string jobCode, int sequence, double amount, string externalRef, bool pinned, string contactEmail, bool approved, string source, string summary, FallbackOffboardingAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public int Sequence { get; set; } = sequence;
    public double Amount { get; set; } = amount;
    public string ExternalRef { get; set; } = externalRef;
    public bool Pinned { get; set; } = pinned;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Approved { get; set; } = approved;
    public string Source { get; set; } = source;
    public string Summary { get; set; } = summary;
    public FallbackOffboardingAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a fallback offboarding allocation in status planned.</summary>
    public static FallbackOffboardingAllocation NewFallbackOffboardingAllocation(string code, string name)
    {
        return new FallbackOffboardingAllocation(
            code,
            name,
            "",
            0,
            0.0,
            "",
            false,
            "",
            false,
            "",
            "",
            FallbackOffboardingAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackOffboardingAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackOffboardingAllocationStatus next)
    {
        switch (Status)
        {
            case FallbackOffboardingAllocationStatus.Planned:
                switch (next)
                {
                    case FallbackOffboardingAllocationStatus.InProgress:
                        return true;
                }
                return false;
            case FallbackOffboardingAllocationStatus.Scheduled:
                switch (next)
                {
                    case FallbackOffboardingAllocationStatus.Completed:
                        return true;
                }
                return false;
            case FallbackOffboardingAllocationStatus.InProgress:
                switch (next)
                {
                    case FallbackOffboardingAllocationStatus.Completed:
                    case FallbackOffboardingAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackOffboardingAllocationStatus.Completed:
                return next == FallbackOffboardingAllocationStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FallbackOffboardingAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback offboarding allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Amount * (double)Sequence, 2);
    }

    public FallbackOffboardingAllocation Copy()
    {
        return new FallbackOffboardingAllocation(
            Code,
            Name,
            JobCode,
            Sequence,
            Amount,
            ExternalRef,
            Pinned,
            ContactEmail,
            Approved,
            Source,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
