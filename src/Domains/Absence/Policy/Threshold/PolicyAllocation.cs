namespace Acme.Workforce.Absence.Policy.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy allocation is in its life cycle.</summary>
public enum PolicyAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a policy allocation and its current status.</summary>
public sealed class PolicyAllocation(string code, string label, List<string> tags, double margin, int updatedAt, bool archived, string name, PolicyAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public double Margin { get; set; } = margin;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Archived { get; set; } = archived;
    public string Name { get; set; } = name;
    public PolicyAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a policy allocation in status requested.</summary>
    public static PolicyAllocation NewPolicyAllocation(string code, string label)
    {
        return new PolicyAllocation(
            code,
            label,
            new List<string>(),
            0.0,
            0,
            false,
            "",
            PolicyAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAllocationStatus next)
    {
        switch (Status)
        {
            case PolicyAllocationStatus.Requested:
                return next == PolicyAllocationStatus.Granted;
            case PolicyAllocationStatus.Granted:
                return next == PolicyAllocationStatus.Revoked;
            case PolicyAllocationStatus.Revoked:
                return next == PolicyAllocationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PolicyAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)UpdatedAt * Margin, 2);
    }

    public PolicyAllocation Copy()
    {
        return new PolicyAllocation(Code, Label, Tags, Margin, UpdatedAt, Archived, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
