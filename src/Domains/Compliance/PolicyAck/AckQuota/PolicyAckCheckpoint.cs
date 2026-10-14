namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack checkpoint is in its life cycle.</summary>
public enum PolicyAckCheckpointStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A policy ack checkpoint as the policy ack team keeps it.</summary>
public sealed class PolicyAckCheckpoint(string code, string title, double margin, int revision, string location, string reviewer, string category, string channel, string unit, double ceiling, PolicyAckCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public int Revision { get; set; } = revision;
    public string Location { get; set; } = location;
    public string Reviewer { get; set; } = reviewer;
    public string Category { get; set; } = category;
    public string Channel { get; set; } = channel;
    public string Unit { get; set; } = unit;
    public double Ceiling { get; set; } = ceiling;
    public PolicyAckCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack checkpoint in status new.</summary>
    public static PolicyAckCheckpoint NewPolicyAckCheckpoint(string code, string title)
    {
        return new PolicyAckCheckpoint(code, title, 0.0, 0, "", "", "", "", "", 0.0, PolicyAckCheckpointStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckCheckpointStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckCheckpointStatus next)
    {
        switch (Status)
        {
            case PolicyAckCheckpointStatus.New:
                switch (next)
                {
                    case PolicyAckCheckpointStatus.Queued:
                    case PolicyAckCheckpointStatus.Failed:
                        return true;
                }
                return false;
            case PolicyAckCheckpointStatus.Queued:
                return next == PolicyAckCheckpointStatus.Running || next == PolicyAckCheckpointStatus.Done;
            case PolicyAckCheckpointStatus.Running:
                return next == PolicyAckCheckpointStatus.Failed;
            case PolicyAckCheckpointStatus.Done:
                return next == PolicyAckCheckpointStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PolicyAckCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetExposure()
    {
        return Math.Round(Margin * (double)Revision, 2);
    }

    public PolicyAckCheckpoint Copy()
    {
        return new PolicyAckCheckpoint(
            Code,
            Title,
            Margin,
            Revision,
            Location,
            Reviewer,
            Category,
            Channel,
            Unit,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
