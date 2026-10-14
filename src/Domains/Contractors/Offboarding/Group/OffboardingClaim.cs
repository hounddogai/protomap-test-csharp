namespace Acme.Workforce.Contractors.Offboarding.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding claim is in its life cycle.</summary>
public enum OffboardingClaimStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A offboarding claim as the offboarding team keeps it.</summary>
public sealed class OffboardingClaim(string code, string name, string phone, string externalRef, string location, double score, OffboardingClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public string ExternalRef { get; set; } = externalRef;
    public string Location { get; set; } = location;
    public double Score { get; set; } = score;
    public OffboardingClaimStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding claim in status idle.</summary>
    public static OffboardingClaim NewOffboardingClaim(string code, string name)
    {
        return new OffboardingClaim(code, name, "", "", "", 0.0, OffboardingClaimStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingClaimStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingClaimStatus next)
    {
        switch (Status)
        {
            case OffboardingClaimStatus.Idle:
                switch (next)
                {
                    case OffboardingClaimStatus.Warming:
                    case OffboardingClaimStatus.Stopped:
                        return true;
                }
                return false;
            case OffboardingClaimStatus.Warming:
                switch (next)
                {
                    case OffboardingClaimStatus.Ready:
                    case OffboardingClaimStatus.Stopped:
                        return true;
                }
                return false;
            case OffboardingClaimStatus.Ready:
                switch (next)
                {
                    case OffboardingClaimStatus.Draining:
                        return true;
                }
                return false;
            case OffboardingClaimStatus.Draining:
                return next == OffboardingClaimStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(OffboardingClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public OffboardingClaim Copy()
    {
        return new OffboardingClaim(Code, Name, Phone, ExternalRef, Location, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
