namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle policy is in its life cycle.</summary>
public enum MeritCyclePolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One merit cycle policy, with what the planners need to act on it.</summary>
public sealed class MeritCyclePolicy(string code, string label, int durationMinutes, int retries, bool approved, List<string> tags, string costCenter, int expiresAt, string location, MeritCyclePolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Retries { get; set; } = retries;
    public bool Approved { get; set; } = approved;
    public List<string> Tags { get; set; } = tags;
    public string CostCenter { get; set; } = costCenter;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Location { get; set; } = location;
    public MeritCyclePolicyStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle policy in status planned.</summary>
    public static MeritCyclePolicy NewMeritCyclePolicy(string code, string label)
    {
        return new MeritCyclePolicy(
            code,
            label,
            0,
            0,
            false,
            new List<string>(),
            "",
            0,
            "",
            MeritCyclePolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCyclePolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCyclePolicyStatus next)
    {
        switch (Status)
        {
            case MeritCyclePolicyStatus.Planned:
                return next == MeritCyclePolicyStatus.InProgress || next == MeritCyclePolicyStatus.Completed;
            case MeritCyclePolicyStatus.Scheduled:
                return next == MeritCyclePolicyStatus.Completed;
            case MeritCyclePolicyStatus.InProgress:
                return next == MeritCyclePolicyStatus.Cancelled;
            case MeritCyclePolicyStatus.Completed:
                return next == MeritCyclePolicyStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(MeritCyclePolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

    public double ProjectedVolume()
    {
        return Math.Round((double)DurationMinutes - (double)ExpiresAt, 2);
    }

    public MeritCyclePolicy Copy()
    {
        return new MeritCyclePolicy(
            Code,
            Label,
            DurationMinutes,
            Retries,
            Approved,
            Tags,
            CostCenter,
            ExpiresAt,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
