namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction policy is in its life cycle.</summary>
public enum DeductionPolicyStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A deduction policy. Its code is unique within a tenant.</summary>
public sealed class DeductionPolicy(string code, string label, string unit, string location, bool visible, string badge, double ratio, double fte, int expiresAt, DeductionPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public string Location { get; set; } = location;
    public bool Visible { get; set; } = visible;
    public string Badge { get; set; } = badge;
    public double Ratio { get; set; } = ratio;
    public double Fte { get; set; } = fte;
    public int ExpiresAt { get; set; } = expiresAt;
    public DeductionPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a deduction policy in status proposed.</summary>
    public static DeductionPolicy NewDeductionPolicy(string code, string label)
    {
        return new DeductionPolicy(code, label, "", "", false, "", 0.0, 0.0, 0, DeductionPolicyStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionPolicyStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionPolicyStatus next)
    {
        switch (Status)
        {
            case DeductionPolicyStatus.Proposed:
                return next == DeductionPolicyStatus.Rejected;
            case DeductionPolicyStatus.Approved:
                switch (next)
                {
                    case DeductionPolicyStatus.Rejected:
                    case DeductionPolicyStatus.Withdrawn:
                        return true;
                }
                return false;
            case DeductionPolicyStatus.Rejected:
                return next == DeductionPolicyStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DeductionPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"deduction policy"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)ExpiresAt * Ratio, 2);
    }

    public DeductionPolicy Copy()
    {
        return new DeductionPolicy(Code, Label, Unit, Location, Visible, Badge, Ratio, Fte, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
