namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy quota is in its life cycle.</summary>
public enum PolicyQuotaStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A policy quota as the policy team keeps it.</summary>
public sealed class PolicyQuota(string code, string name, int revision, int capacity, double maximum, string summary, string title, bool visible, string department, int batchSize, int leaveDays, PolicyQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public int Capacity { get; set; } = capacity;
    public double Maximum { get; set; } = maximum;
    public string Summary { get; set; } = summary;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public string Department { get; set; } = department;
    public int BatchSize { get; set; } = batchSize;
    public int LeaveDays { get; set; } = leaveDays;
    public PolicyQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a policy quota in status proposed.</summary>
    public static PolicyQuota NewPolicyQuota(string code, string name)
    {
        return new PolicyQuota(code, name, 0, 0, 0.0, "", "", false, "", 0, 0, PolicyQuotaStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyQuotaStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyQuotaStatus next)
    {
        switch (Status)
        {
            case PolicyQuotaStatus.Proposed:
                switch (next)
                {
                    case PolicyQuotaStatus.Approved:
                    case PolicyQuotaStatus.Rejected:
                        return true;
                }
                return false;
            case PolicyQuotaStatus.Approved:
                switch (next)
                {
                    case PolicyQuotaStatus.Rejected:
                    case PolicyQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
            case PolicyQuotaStatus.Rejected:
                return next == PolicyQuotaStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PolicyQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Maximum + (double)BatchSize, 2);
    }

    public PolicyQuota Copy()
    {
        return new PolicyQuota(
            Code,
            Name,
            Revision,
            Capacity,
            Maximum,
            Summary,
            Title,
            Visible,
            Department,
            BatchSize,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
