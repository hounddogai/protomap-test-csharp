namespace Acme.Workforce.Compliance.PolicyAck.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual policy ack group is in its life cycle.</summary>
public enum ManualPolicyAckGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a manual policy ack group and its current status.</summary>
public sealed class ManualPolicyAckGroup(string code, string name, double margin, double hours, int version, string ownerId, string timezone, string managerId, string contactEmail, double minimum, ManualPolicyAckGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public double Hours { get; set; } = hours;
    public int Version { get; set; } = version;
    public string OwnerId { get; set; } = ownerId;
    public string Timezone { get; set; } = timezone;
    public string ManagerId { get; set; } = managerId;
    public string ContactEmail { get; set; } = contactEmail;
    public double Minimum { get; set; } = minimum;
    public ManualPolicyAckGroupStatus Status { get; set; } = status;

    /// <summary>Starts a manual policy ack group in status idle.</summary>
    public static ManualPolicyAckGroup NewManualPolicyAckGroup(string code, string name)
    {
        return new ManualPolicyAckGroup(
            code,
            name,
            0.0,
            0.0,
            0,
            "",
            "",
            "",
            "",
            0.0,
            ManualPolicyAckGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPolicyAckGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPolicyAckGroupStatus next)
    {
        switch (Status)
        {
            case ManualPolicyAckGroupStatus.Idle:
                return next == ManualPolicyAckGroupStatus.Warming || next == ManualPolicyAckGroupStatus.Ready;
            case ManualPolicyAckGroupStatus.Warming:
                return next == ManualPolicyAckGroupStatus.Stopped;
            case ManualPolicyAckGroupStatus.Ready:
                return next == ManualPolicyAckGroupStatus.Draining || next == ManualPolicyAckGroupStatus.Stopped;
            case ManualPolicyAckGroupStatus.Draining:
                return next == ManualPolicyAckGroupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ManualPolicyAckGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual policy ack group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"manual policy ack group"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Margin + Minimum, 2);
    }

    public ManualPolicyAckGroup Copy()
    {
        return new ManualPolicyAckGroup(
            Code,
            Name,
            Margin,
            Hours,
            Version,
            OwnerId,
            Timezone,
            ManagerId,
            ContactEmail,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
