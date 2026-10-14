namespace Acme.Workforce.Compliance.Attestation.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation policy is in its life cycle.</summary>
public enum AttestationPolicyStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a attestation policy and its current status.</summary>
public sealed class AttestationPolicy(string code, string name, double margin, int sequence, bool visible, string salaryBand, string title, double discount, string unit, string label, AttestationPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public int Sequence { get; set; } = sequence;
    public bool Visible { get; set; } = visible;
    public string SalaryBand { get; set; } = salaryBand;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string Unit { get; set; } = unit;
    public string Label { get; set; } = label;
    public AttestationPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a attestation policy in status idle.</summary>
    public static AttestationPolicy NewAttestationPolicy(string code, string name)
    {
        return new AttestationPolicy(code, name, 0.0, 0, false, "", "", 0.0, "", "", AttestationPolicyStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationPolicyStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationPolicyStatus next)
    {
        switch (Status)
        {
            case AttestationPolicyStatus.Idle:
                switch (next)
                {
                    case AttestationPolicyStatus.Warming:
                    case AttestationPolicyStatus.Draining:
                        return true;
                }
                return false;
            case AttestationPolicyStatus.Warming:
                return next == AttestationPolicyStatus.Draining || next == AttestationPolicyStatus.Stopped;
            case AttestationPolicyStatus.Ready:
                return next == AttestationPolicyStatus.Draining;
            case AttestationPolicyStatus.Draining:
                return next == AttestationPolicyStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AttestationPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Margin * (double)Sequence, 2);
    }

    public AttestationPolicy Copy()
    {
        return new AttestationPolicy(
            Code,
            Name,
            Margin,
            Sequence,
            Visible,
            SalaryBand,
            Title,
            Discount,
            Unit,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
