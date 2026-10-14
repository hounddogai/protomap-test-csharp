namespace Acme.Workforce.Absence.Policy.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy mapping is in its life cycle.</summary>
public enum PolicyMappingStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A policy mapping as the policy team keeps it.</summary>
public sealed class PolicyMapping(string code, string name, string ownerId, bool visible, string unit, string summary, int limit, PolicyMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public bool Visible { get; set; } = visible;
    public string Unit { get; set; } = unit;
    public string Summary { get; set; } = summary;
    public int Limit { get; set; } = limit;
    public PolicyMappingStatus Status { get; set; } = status;

    /// <summary>Starts a policy mapping in status new.</summary>
    public static PolicyMapping NewPolicyMapping(string code, string name)
    {
        return new PolicyMapping(code, name, "", false, "", "", 0, PolicyMappingStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyMappingStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyMappingStatus next)
    {
        switch (Status)
        {
            case PolicyMappingStatus.New:
                return next == PolicyMappingStatus.Running || next == PolicyMappingStatus.Done;
            case PolicyMappingStatus.Queued:
                return next == PolicyMappingStatus.Done;
            case PolicyMappingStatus.Running:
                switch (next)
                {
                    case PolicyMappingStatus.Done:
                        return true;
                }
                return false;
            case PolicyMappingStatus.Done:
                return next == PolicyMappingStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PolicyMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public PolicyMapping Copy()
    {
        return new PolicyMapping(Code, Name, OwnerId, Visible, Unit, Summary, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
