namespace Acme.Workforce.Compliance.Attestation.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default attestation allocation is in its life cycle.</summary>
public enum DefaultAttestationAllocationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A default attestation allocation. Its code is unique within a tenant.</summary>
public sealed class DefaultAttestationAllocation(string code, string title, bool visible, string managerId, double ceiling, string badge, string employeeId, int graceDays, DefaultAttestationAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public string ManagerId { get; set; } = managerId;
    public double Ceiling { get; set; } = ceiling;
    public string Badge { get; set; } = badge;
    public string EmployeeId { get; set; } = employeeId;
    public int GraceDays { get; set; } = graceDays;
    public DefaultAttestationAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a default attestation allocation in status new.</summary>
    public static DefaultAttestationAllocation NewDefaultAttestationAllocation(string code, string title)
    {
        return new DefaultAttestationAllocation(
            code,
            title,
            false,
            "",
            0.0,
            "",
            "",
            0,
            DefaultAttestationAllocationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAttestationAllocationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAttestationAllocationStatus next)
    {
        switch (Status)
        {
            case DefaultAttestationAllocationStatus.New:
                return next == DefaultAttestationAllocationStatus.Failed;
            case DefaultAttestationAllocationStatus.Queued:
                switch (next)
                {
                    case DefaultAttestationAllocationStatus.Running:
                    case DefaultAttestationAllocationStatus.Failed:
                        return true;
                }
                return false;
            case DefaultAttestationAllocationStatus.Running:
                return next == DefaultAttestationAllocationStatus.Done;
            case DefaultAttestationAllocationStatus.Done:
                switch (next)
                {
                    case DefaultAttestationAllocationStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultAttestationAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default attestation allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default attestation allocation"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ceiling - (double)GraceDays, 2);
    }

    public DefaultAttestationAllocation Copy()
    {
        return new DefaultAttestationAllocation(
            Code,
            Title,
            Visible,
            ManagerId,
            Ceiling,
            Badge,
            EmployeeId,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
