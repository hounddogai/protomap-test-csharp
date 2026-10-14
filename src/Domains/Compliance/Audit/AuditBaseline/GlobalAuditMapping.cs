namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global audit mapping is in its life cycle.</summary>
public enum GlobalAuditMappingStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One global audit mapping, with what the planners need to act on it.</summary>
public sealed class GlobalAuditMapping(string code, string name, int retries, string department, double weight, double budget, GlobalAuditMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string Department { get; set; } = department;
    public double Weight { get; set; } = weight;
    public double Budget { get; set; } = budget;
    public GlobalAuditMappingStatus Status { get; set; } = status;

    /// <summary>Starts a global audit mapping in status idle.</summary>
    public static GlobalAuditMapping NewGlobalAuditMapping(string code, string name)
    {
        return new GlobalAuditMapping(code, name, 0, "", 0.0, 0.0, GlobalAuditMappingStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAuditMappingStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAuditMappingStatus next)
    {
        switch (Status)
        {
            case GlobalAuditMappingStatus.Idle:
                switch (next)
                {
                    case GlobalAuditMappingStatus.Draining:
                        return true;
                }
                return false;
            case GlobalAuditMappingStatus.Warming:
                switch (next)
                {
                    case GlobalAuditMappingStatus.Stopped:
                        return true;
                }
                return false;
            case GlobalAuditMappingStatus.Ready:
                return next == GlobalAuditMappingStatus.Draining || next == GlobalAuditMappingStatus.Stopped;
            case GlobalAuditMappingStatus.Draining:
                return next == GlobalAuditMappingStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(GlobalAuditMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global audit mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Weight - Budget, 2);
    }

    public GlobalAuditMapping Copy()
    {
        return new GlobalAuditMapping(Code, Name, Retries, Department, Weight, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
