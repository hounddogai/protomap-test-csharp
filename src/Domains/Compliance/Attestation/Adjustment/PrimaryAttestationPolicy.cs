namespace Acme.Workforce.Compliance.Attestation.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary attestation policy is in its life cycle.</summary>
public enum PrimaryAttestationPolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a primary attestation policy and its current status.</summary>
public sealed class PrimaryAttestationPolicy(string code, string name, double discount, string badge, int limit, PrimaryAttestationPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public string Badge { get; set; } = badge;
    public int Limit { get; set; } = limit;
    public PrimaryAttestationPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a primary attestation policy in status planned.</summary>
    public static PrimaryAttestationPolicy NewPrimaryAttestationPolicy(string code, string name)
    {
        return new PrimaryAttestationPolicy(code, name, 0.0, "", 0, PrimaryAttestationPolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAttestationPolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAttestationPolicyStatus next)
    {
        switch (Status)
        {
            case PrimaryAttestationPolicyStatus.Planned:
                switch (next)
                {
                    case PrimaryAttestationPolicyStatus.InProgress:
                        return true;
                }
                return false;
            case PrimaryAttestationPolicyStatus.Scheduled:
                switch (next)
                {
                    case PrimaryAttestationPolicyStatus.InProgress:
                    case PrimaryAttestationPolicyStatus.Completed:
                        return true;
                }
                return false;
            case PrimaryAttestationPolicyStatus.InProgress:
                switch (next)
                {
                    case PrimaryAttestationPolicyStatus.Completed:
                        return true;
                }
                return false;
            case PrimaryAttestationPolicyStatus.Completed:
                switch (next)
                {
                    case PrimaryAttestationPolicyStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryAttestationPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary attestation policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary attestation policy"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Limit - Discount, 2);
    }

    public PrimaryAttestationPolicy Copy()
    {
        return new PrimaryAttestationPolicy(Code, Name, Discount, Badge, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
