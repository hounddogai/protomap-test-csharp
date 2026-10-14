namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate quota is in its life cycle.</summary>
public enum RateQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A rate quota. Its code is unique within a tenant.</summary>
public sealed class RateQuota(string code, string name, int version, string description, string department, string channel, RateQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string Description { get; set; } = description;
    public string Department { get; set; } = department;
    public string Channel { get; set; } = channel;
    public RateQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a rate quota in status planned.</summary>
    public static RateQuota NewRateQuota(string code, string name)
    {
        return new RateQuota(code, name, 0, "", "", "", RateQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateQuotaStatus next)
    {
        switch (Status)
        {
            case RateQuotaStatus.Planned:
                return next == RateQuotaStatus.InProgress || next == RateQuotaStatus.Completed;
            case RateQuotaStatus.Scheduled:
                return next == RateQuotaStatus.Completed;
            case RateQuotaStatus.InProgress:
                return next == RateQuotaStatus.Completed;
            case RateQuotaStatus.Completed:
                return next == RateQuotaStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RateQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"rate quota"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public RateQuota Copy()
    {
        return new RateQuota(Code, Name, Version, Description, Department, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
