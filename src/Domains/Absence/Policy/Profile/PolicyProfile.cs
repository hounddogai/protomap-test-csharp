namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy profile is in its life cycle.</summary>
public enum PolicyProfileStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A policy profile. Its code is unique within a tenant.</summary>
public sealed class PolicyProfile(string code, string label, string costCenter, int graceDays, string category, PolicyProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public int GraceDays { get; set; } = graceDays;
    public string Category { get; set; } = category;
    public PolicyProfileStatus Status { get; set; } = status;

    /// <summary>Starts a policy profile in status draft.</summary>
    public static PolicyProfile NewPolicyProfile(string code, string label)
    {
        return new PolicyProfile(code, label, "", 0, "", PolicyProfileStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyProfileStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyProfileStatus next)
    {
        switch (Status)
        {
            case PolicyProfileStatus.Draft:
                return next == PolicyProfileStatus.Active || next == PolicyProfileStatus.Suspended;
            case PolicyProfileStatus.Pending:
                return next == PolicyProfileStatus.Closed;
            case PolicyProfileStatus.Active:
                switch (next)
                {
                    case PolicyProfileStatus.Suspended:
                    case PolicyProfileStatus.Closed:
                        return true;
                }
                return false;
            case PolicyProfileStatus.Suspended:
                switch (next)
                {
                    case PolicyProfileStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"policy profile"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)GraceDays;
    }

    public PolicyProfile Copy()
    {
        return new PolicyProfile(Code, Label, CostCenter, GraceDays, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
