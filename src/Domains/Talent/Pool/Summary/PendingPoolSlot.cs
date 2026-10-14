namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending pool slot is in its life cycle.</summary>
public enum PendingPoolSlotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pending pool slot and its current status.</summary>
public sealed class PendingPoolSlot(string code, string label, List<string> tags, int updatedAt, bool visible, string salaryBand, int version, string jobCode, int createdAt, string channel, PendingPoolSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Visible { get; set; } = visible;
    public string SalaryBand { get; set; } = salaryBand;
    public int Version { get; set; } = version;
    public string JobCode { get; set; } = jobCode;
    public int CreatedAt { get; set; } = createdAt;
    public string Channel { get; set; } = channel;
    public PendingPoolSlotStatus Status { get; set; } = status;

    /// <summary>Starts a pending pool slot in status idle.</summary>
    public static PendingPoolSlot NewPendingPoolSlot(string code, string label)
    {
        return new PendingPoolSlot(
            code,
            label,
            new List<string>(),
            0,
            false,
            "",
            0,
            "",
            0,
            "",
            PendingPoolSlotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPoolSlotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPoolSlotStatus next)
    {
        switch (Status)
        {
            case PendingPoolSlotStatus.Idle:
                switch (next)
                {
                    case PendingPoolSlotStatus.Warming:
                        return true;
                }
                return false;
            case PendingPoolSlotStatus.Warming:
                switch (next)
                {
                    case PendingPoolSlotStatus.Draining:
                        return true;
                }
                return false;
            case PendingPoolSlotStatus.Ready:
                switch (next)
                {
                    case PendingPoolSlotStatus.Stopped:
                        return true;
                }
                return false;
            case PendingPoolSlotStatus.Draining:
                switch (next)
                {
                    case PendingPoolSlotStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingPoolSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending pool slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"pending pool slot"} {Code}";
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

    public double NetVolume()
    {
        return Math.Round((double)CreatedAt + (double)Version, 2);
    }

    public PendingPoolSlot Copy()
    {
        return new PendingPoolSlot(
            Code,
            Label,
            Tags,
            UpdatedAt,
            Visible,
            SalaryBand,
            Version,
            JobCode,
            CreatedAt,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
