namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy window is in its life cycle.</summary>
public enum PolicyWindowStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One policy window, with what the planners need to act on it.</summary>
public sealed class PolicyWindow(string code, string name, string employeeId, double cost, int limit, PolicyWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public double Cost { get; set; } = cost;
    public int Limit { get; set; } = limit;
    public PolicyWindowStatus Status { get; set; } = status;

    /// <summary>Starts a policy window in status created.</summary>
    public static PolicyWindow NewPolicyWindow(string code, string name)
    {
        return new PolicyWindow(code, name, "", 0.0, 0, PolicyWindowStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyWindowStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyWindowStatus next)
    {
        switch (Status)
        {
            case PolicyWindowStatus.Created:
                return next == PolicyWindowStatus.Published || next == PolicyWindowStatus.Retired;
            case PolicyWindowStatus.Validated:
                switch (next)
                {
                    case PolicyWindowStatus.Published:
                    case PolicyWindowStatus.Retired:
                        return true;
                }
                return false;
            case PolicyWindowStatus.Published:
                switch (next)
                {
                    case PolicyWindowStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetValue()
    {
        return Math.Round(Cost - (double)Limit, 2);
    }

    public PolicyWindow Copy()
    {
        return new PolicyWindow(Code, Name, EmployeeId, Cost, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
