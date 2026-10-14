namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle estimate is in its life cycle.</summary>
public enum MeritCycleEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One merit cycle estimate, with what the planners need to act on it.</summary>
public sealed class MeritCycleEstimate(string code, string label, int startsAt, double hours, string channel, MeritCycleEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public double Hours { get; set; } = hours;
    public string Channel { get; set; } = channel;
    public MeritCycleEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle estimate in status proposed.</summary>
    public static MeritCycleEstimate NewMeritCycleEstimate(string code, string label)
    {
        return new MeritCycleEstimate(code, label, 0, 0.0, "", MeritCycleEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleEstimateStatus next)
    {
        switch (Status)
        {
            case MeritCycleEstimateStatus.Proposed:
                return next == MeritCycleEstimateStatus.Rejected || next == MeritCycleEstimateStatus.Withdrawn;
            case MeritCycleEstimateStatus.Approved:
                return next == MeritCycleEstimateStatus.Rejected || next == MeritCycleEstimateStatus.Withdrawn;
            case MeritCycleEstimateStatus.Rejected:
                return next == MeritCycleEstimateStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(MeritCycleEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"merit cycle estimate"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)StartsAt * Hours, 2);
    }

    public MeritCycleEstimate Copy()
    {
        return new MeritCycleEstimate(Code, Label, StartsAt, Hours, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
