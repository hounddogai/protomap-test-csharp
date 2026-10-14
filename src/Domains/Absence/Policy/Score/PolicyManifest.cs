namespace Acme.Workforce.Absence.Policy.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy manifest is in its life cycle.</summary>
public enum PolicyManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One policy manifest, with what the planners need to act on it.</summary>
public sealed class PolicyManifest(string code, string name, string employeeId, double threshold, string category, double cost, int endsAt, string description, string channel, int limit, PolicyManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public double Threshold { get; set; } = threshold;
    public string Category { get; set; } = category;
    public double Cost { get; set; } = cost;
    public int EndsAt { get; set; } = endsAt;
    public string Description { get; set; } = description;
    public string Channel { get; set; } = channel;
    public int Limit { get; set; } = limit;
    public PolicyManifestStatus Status { get; set; } = status;

    /// <summary>Starts a policy manifest in status proposed.</summary>
    public static PolicyManifest NewPolicyManifest(string code, string name)
    {
        return new PolicyManifest(code, name, "", 0.0, "", 0.0, 0, "", "", 0, PolicyManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyManifestStatus next)
    {
        switch (Status)
        {
            case PolicyManifestStatus.Proposed:
                return next == PolicyManifestStatus.Approved || next == PolicyManifestStatus.Rejected;
            case PolicyManifestStatus.Approved:
                return next == PolicyManifestStatus.Withdrawn;
            case PolicyManifestStatus.Rejected:
                switch (next)
                {
                    case PolicyManifestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)EndsAt + Threshold, 2);
    }

    public PolicyManifest Copy()
    {
        return new PolicyManifest(
            Code,
            Name,
            EmployeeId,
            Threshold,
            Category,
            Cost,
            EndsAt,
            Description,
            Channel,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
