namespace Acme.Workforce.Compliance.PolicyAck.AckGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict policy ack group is in its life cycle.</summary>
public enum StrictPolicyAckGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A strict policy ack group. Its code is unique within a tenant.</summary>
public sealed class StrictPolicyAckGroup(string code, string name, string department, string unit, int expiresAt, int sequence, int startsAt, StrictPolicyAckGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public string Unit { get; set; } = unit;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Sequence { get; set; } = sequence;
    public int StartsAt { get; set; } = startsAt;
    public StrictPolicyAckGroupStatus Status { get; set; } = status;

    /// <summary>Starts a strict policy ack group in status created.</summary>
    public static StrictPolicyAckGroup NewStrictPolicyAckGroup(string code, string name)
    {
        return new StrictPolicyAckGroup(code, name, "", "", 0, 0, 0, StrictPolicyAckGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPolicyAckGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPolicyAckGroupStatus next)
    {
        switch (Status)
        {
            case StrictPolicyAckGroupStatus.Created:
                switch (next)
                {
                    case StrictPolicyAckGroupStatus.Validated:
                    case StrictPolicyAckGroupStatus.Published:
                        return true;
                }
                return false;
            case StrictPolicyAckGroupStatus.Validated:
                return next == StrictPolicyAckGroupStatus.Published || next == StrictPolicyAckGroupStatus.Retired;
            case StrictPolicyAckGroupStatus.Published:
                switch (next)
                {
                    case StrictPolicyAckGroupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictPolicyAckGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict policy ack group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)StartsAt - (double)ExpiresAt, 2);
    }

    public StrictPolicyAckGroup Copy()
    {
        return new StrictPolicyAckGroup(Code, Name, Department, Unit, ExpiresAt, Sequence, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
