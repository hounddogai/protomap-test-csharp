namespace Acme.Workforce.Absence.Balance.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global balance policy is in its life cycle.</summary>
public enum GlobalBalancePolicyStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A global balance policy. Its code is unique within a tenant.</summary>
public sealed class GlobalBalancePolicy(string code, string label, int endsAt, bool visible, int graceDays, string costCenter, string contactEmail, GlobalBalancePolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public bool Visible { get; set; } = visible;
    public int GraceDays { get; set; } = graceDays;
    public string CostCenter { get; set; } = costCenter;
    public string ContactEmail { get; set; } = contactEmail;
    public GlobalBalancePolicyStatus Status { get; set; } = status;

    /// <summary>Starts a global balance policy in status proposed.</summary>
    public static GlobalBalancePolicy NewGlobalBalancePolicy(string code, string label)
    {
        return new GlobalBalancePolicy(code, label, 0, false, 0, "", "", GlobalBalancePolicyStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalBalancePolicyStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalBalancePolicyStatus next)
    {
        switch (Status)
        {
            case GlobalBalancePolicyStatus.Proposed:
                return next == GlobalBalancePolicyStatus.Withdrawn;
            case GlobalBalancePolicyStatus.Approved:
                switch (next)
                {
                    case GlobalBalancePolicyStatus.Withdrawn:
                        return true;
                }
                return false;
            case GlobalBalancePolicyStatus.Rejected:
                return next == GlobalBalancePolicyStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(GlobalBalancePolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global balance policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)EndsAt * (double)GraceDays, 2);
    }

    public GlobalBalancePolicy Copy()
    {
        return new GlobalBalancePolicy(Code, Label, EndsAt, Visible, GraceDays, CostCenter, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
