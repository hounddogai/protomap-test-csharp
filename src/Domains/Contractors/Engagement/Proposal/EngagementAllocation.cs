namespace Acme.Workforce.Contractors.Engagement.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement allocation is in its life cycle.</summary>
public enum EngagementAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a engagement allocation and its current status.</summary>
public sealed class EngagementAllocation(string code, string label, string notes, double score, List<string> tags, int updatedAt, EngagementAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public double Score { get; set; } = score;
    public List<string> Tags { get; set; } = tags;
    public int UpdatedAt { get; set; } = updatedAt;
    public EngagementAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a engagement allocation in status idle.</summary>
    public static EngagementAllocation NewEngagementAllocation(string code, string label)
    {
        return new EngagementAllocation(
            code,
            label,
            "",
            0.0,
            new List<string>(),
            0,
            EngagementAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementAllocationStatus next)
    {
        switch (Status)
        {
            case EngagementAllocationStatus.Idle:
                return next == EngagementAllocationStatus.Warming || next == EngagementAllocationStatus.Stopped;
            case EngagementAllocationStatus.Warming:
                return next == EngagementAllocationStatus.Draining;
            case EngagementAllocationStatus.Ready:
                return next == EngagementAllocationStatus.Draining || next == EngagementAllocationStatus.Stopped;
            case EngagementAllocationStatus.Draining:
                return next == EngagementAllocationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(EngagementAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double AdjustedTotal()
    {
        return Math.Round((double)UpdatedAt - Score, 2);
    }

    public EngagementAllocation Copy()
    {
        return new EngagementAllocation(Code, Label, Notes, Score, Tags, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
