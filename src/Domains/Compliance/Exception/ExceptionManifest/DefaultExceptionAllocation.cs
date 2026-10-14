namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default exception allocation is in its life cycle.</summary>
public enum DefaultExceptionAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A default exception allocation. Its code is unique within a tenant.</summary>
public sealed class DefaultExceptionAllocation(string code, string name, int version, int leaveDays, string externalRef, DefaultExceptionAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public int LeaveDays { get; set; } = leaveDays;
    public string ExternalRef { get; set; } = externalRef;
    public DefaultExceptionAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a default exception allocation in status planned.</summary>
    public static DefaultExceptionAllocation NewDefaultExceptionAllocation(string code, string name)
    {
        return new DefaultExceptionAllocation(code, name, 0, 0, "", DefaultExceptionAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultExceptionAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultExceptionAllocationStatus next)
    {
        switch (Status)
        {
            case DefaultExceptionAllocationStatus.Planned:
                return next == DefaultExceptionAllocationStatus.Cancelled;
            case DefaultExceptionAllocationStatus.Scheduled:
                return next == DefaultExceptionAllocationStatus.InProgress || next == DefaultExceptionAllocationStatus.Completed;
            case DefaultExceptionAllocationStatus.InProgress:
                return next == DefaultExceptionAllocationStatus.Completed;
            case DefaultExceptionAllocationStatus.Completed:
                switch (next)
                {
                    case DefaultExceptionAllocationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultExceptionAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default exception allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"default exception allocation"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)LeaveDays * (double)Version, 2);
    }

    public DefaultExceptionAllocation Copy()
    {
        return new DefaultExceptionAllocation(Code, Name, Version, LeaveDays, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
