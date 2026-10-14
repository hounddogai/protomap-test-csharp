namespace Acme.Workforce.Compliance.Certification.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification cycle is in its life cycle.</summary>
public enum CertificationCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A certification cycle as the certification team keeps it.</summary>
public sealed class CertificationCycle(string code, string label, int capacity, int createdAt, double amount, int expiresAt, Dictionary<string, string> attributes, int attempts, CertificationCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public int CreatedAt { get; set; } = createdAt;
    public double Amount { get; set; } = amount;
    public int ExpiresAt { get; set; } = expiresAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Attempts { get; set; } = attempts;
    public CertificationCycleStatus Status { get; set; } = status;

    /// <summary>Starts a certification cycle in status proposed.</summary>
    public static CertificationCycle NewCertificationCycle(string code, string label)
    {
        return new CertificationCycle(
            code,
            label,
            0,
            0,
            0.0,
            0,
            new Dictionary<string, string>(),
            0,
            CertificationCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationCycleStatus next)
    {
        switch (Status)
        {
            case CertificationCycleStatus.Proposed:
                return next == CertificationCycleStatus.Rejected || next == CertificationCycleStatus.Withdrawn;
            case CertificationCycleStatus.Approved:
                switch (next)
                {
                    case CertificationCycleStatus.Rejected:
                    case CertificationCycleStatus.Withdrawn:
                        return true;
                }
                return false;
            case CertificationCycleStatus.Rejected:
                switch (next)
                {
                    case CertificationCycleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetVolume()
    {
        return Math.Round(Amount * (double)Capacity, 2);
    }

    public CertificationCycle Copy()
    {
        return new CertificationCycle(
            Code,
            Label,
            Capacity,
            CreatedAt,
            Amount,
            ExpiresAt,
            Attributes,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
