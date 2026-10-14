namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack preference is in its life cycle.</summary>
public enum PolicyAckPreferenceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One policy ack preference, with what the planners need to act on it.</summary>
public sealed class PolicyAckPreference(string code, string name, string employeeId, double margin, double floorValue, int limit, string managerId, PolicyAckPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public double Margin { get; set; } = margin;
    public double FloorValue { get; set; } = floorValue;
    public int Limit { get; set; } = limit;
    public string ManagerId { get; set; } = managerId;
    public PolicyAckPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack preference in status idle.</summary>
    public static PolicyAckPreference NewPolicyAckPreference(string code, string name)
    {
        return new PolicyAckPreference(code, name, "", 0.0, 0.0, 0, "", PolicyAckPreferenceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckPreferenceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckPreferenceStatus next)
    {
        switch (Status)
        {
            case PolicyAckPreferenceStatus.Idle:
                switch (next)
                {
                    case PolicyAckPreferenceStatus.Warming:
                    case PolicyAckPreferenceStatus.Ready:
                        return true;
                }
                return false;
            case PolicyAckPreferenceStatus.Warming:
                switch (next)
                {
                    case PolicyAckPreferenceStatus.Ready:
                    case PolicyAckPreferenceStatus.Stopped:
                        return true;
                }
                return false;
            case PolicyAckPreferenceStatus.Ready:
                switch (next)
                {
                    case PolicyAckPreferenceStatus.Stopped:
                        return true;
                }
                return false;
            case PolicyAckPreferenceStatus.Draining:
                switch (next)
                {
                    case PolicyAckPreferenceStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Margin - FloorValue, 2);
    }

    public PolicyAckPreference Copy()
    {
        return new PolicyAckPreference(Code, Name, EmployeeId, Margin, FloorValue, Limit, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
