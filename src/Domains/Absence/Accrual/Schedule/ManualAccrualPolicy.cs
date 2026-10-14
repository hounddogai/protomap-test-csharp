namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual accrual policy is in its life cycle.</summary>
public enum ManualAccrualPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One manual accrual policy, with what the planners need to act on it.</summary>
public sealed class ManualAccrualPolicy(string code, string name, string externalRef, int expiresAt, string badge, string source, int quantity, ManualAccrualPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Badge { get; set; } = badge;
    public string Source { get; set; } = source;
    public int Quantity { get; set; } = quantity;
    public ManualAccrualPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a manual accrual policy in status created.</summary>
    public static ManualAccrualPolicy NewManualAccrualPolicy(string code, string name)
    {
        return new ManualAccrualPolicy(code, name, "", 0, "", "", 0, ManualAccrualPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAccrualPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAccrualPolicyStatus next)
    {
        switch (Status)
        {
            case ManualAccrualPolicyStatus.Created:
                switch (next)
                {
                    case ManualAccrualPolicyStatus.Published:
                    case ManualAccrualPolicyStatus.Retired:
                        return true;
                }
                return false;
            case ManualAccrualPolicyStatus.Validated:
                return next == ManualAccrualPolicyStatus.Retired;
            case ManualAccrualPolicyStatus.Published:
                return next == ManualAccrualPolicyStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ManualAccrualPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual accrual policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Quantity + (double)ExpiresAt, 2);
    }

    public ManualAccrualPolicy Copy()
    {
        return new ManualAccrualPolicy(Code, Name, ExternalRef, ExpiresAt, Badge, Source, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
