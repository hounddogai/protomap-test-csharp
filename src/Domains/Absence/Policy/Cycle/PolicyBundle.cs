namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy bundle is in its life cycle.</summary>
public enum PolicyBundleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A policy bundle as the policy team keeps it.</summary>
public sealed class PolicyBundle(string code, string label, string costCenter, string unit, string department, string title, int capacity, string currency, string contactEmail, double floorValue, PolicyBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public string Unit { get; set; } = unit;
    public string Department { get; set; } = department;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public string Currency { get; set; } = currency;
    public string ContactEmail { get; set; } = contactEmail;
    public double FloorValue { get; set; } = floorValue;
    public PolicyBundleStatus Status { get; set; } = status;

    /// <summary>Starts a policy bundle in status idle.</summary>
    public static PolicyBundle NewPolicyBundle(string code, string label)
    {
        return new PolicyBundle(code, label, "", "", "", "", 0, "", "", 0.0, PolicyBundleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyBundleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyBundleStatus next)
    {
        switch (Status)
        {
            case PolicyBundleStatus.Idle:
                return next == PolicyBundleStatus.Warming;
            case PolicyBundleStatus.Warming:
                switch (next)
                {
                    case PolicyBundleStatus.Ready:
                        return true;
                }
                return false;
            case PolicyBundleStatus.Ready:
                switch (next)
                {
                    case PolicyBundleStatus.Draining:
                        return true;
                }
                return false;
            case PolicyBundleStatus.Draining:
                return next == PolicyBundleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PolicyBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Capacity + FloorValue, 2);
    }

    public PolicyBundle Copy()
    {
        return new PolicyBundle(
            Code,
            Label,
            CostCenter,
            Unit,
            Department,
            Title,
            Capacity,
            Currency,
            ContactEmail,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
