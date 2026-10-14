namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary nomination policy is in its life cycle.</summary>
public enum PrimaryNominationPolicyStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A primary nomination policy. Its code is unique within a tenant.</summary>
public sealed class PrimaryNominationPolicy(string code, string title, double ceiling, string location, double budget, PrimaryNominationPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public string Location { get; set; } = location;
    public double Budget { get; set; } = budget;
    public PrimaryNominationPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a primary nomination policy in status idle.</summary>
    public static PrimaryNominationPolicy NewPrimaryNominationPolicy(string code, string title)
    {
        return new PrimaryNominationPolicy(code, title, 0.0, "", 0.0, PrimaryNominationPolicyStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryNominationPolicyStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryNominationPolicyStatus next)
    {
        switch (Status)
        {
            case PrimaryNominationPolicyStatus.Idle:
                return next == PrimaryNominationPolicyStatus.Ready || next == PrimaryNominationPolicyStatus.Stopped;
            case PrimaryNominationPolicyStatus.Warming:
                return next == PrimaryNominationPolicyStatus.Draining;
            case PrimaryNominationPolicyStatus.Ready:
                switch (next)
                {
                    case PrimaryNominationPolicyStatus.Draining:
                    case PrimaryNominationPolicyStatus.Stopped:
                        return true;
                }
                return false;
            case PrimaryNominationPolicyStatus.Draining:
                switch (next)
                {
                    case PrimaryNominationPolicyStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryNominationPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary nomination policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Ceiling * Budget, 2);
    }

    public PrimaryNominationPolicy Copy()
    {
        return new PrimaryNominationPolicy(Code, Title, Ceiling, Location, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
