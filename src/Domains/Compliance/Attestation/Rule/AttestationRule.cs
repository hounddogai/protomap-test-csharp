namespace Acme.Workforce.Compliance.Attestation.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation rule is in its life cycle.</summary>
public enum AttestationRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a attestation rule and its current status.</summary>
public sealed class AttestationRule(string code, string title, double rate, bool archived, double rating, AttestationRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public bool Archived { get; set; } = archived;
    public double Rating { get; set; } = rating;
    public AttestationRuleStatus Status { get; set; } = status;

    /// <summary>Starts a attestation rule in status idle.</summary>
    public static AttestationRule NewAttestationRule(string code, string title)
    {
        return new AttestationRule(code, title, 0.0, false, 0.0, AttestationRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationRuleStatus next)
    {
        switch (Status)
        {
            case AttestationRuleStatus.Idle:
                return next == AttestationRuleStatus.Stopped;
            case AttestationRuleStatus.Warming:
                return next == AttestationRuleStatus.Ready || next == AttestationRuleStatus.Draining;
            case AttestationRuleStatus.Ready:
                switch (next)
                {
                    case AttestationRuleStatus.Stopped:
                        return true;
                }
                return false;
            case AttestationRuleStatus.Draining:
                return next == AttestationRuleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AttestationRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Rate + Rating, 2);
    }

    public AttestationRule Copy()
    {
        return new AttestationRule(Code, Title, Rate, Archived, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
