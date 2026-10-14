namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle milestone is in its life cycle.</summary>
public enum MeritCycleMilestoneStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a merit cycle milestone and its current status.</summary>
public sealed class MeritCycleMilestone(string code, string label, string summary, bool visible, double threshold, int batchSize, int revision, string externalRef, MeritCycleMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public bool Visible { get; set; } = visible;
    public double Threshold { get; set; } = threshold;
    public int BatchSize { get; set; } = batchSize;
    public int Revision { get; set; } = revision;
    public string ExternalRef { get; set; } = externalRef;
    public MeritCycleMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle milestone in status requested.</summary>
    public static MeritCycleMilestone NewMeritCycleMilestone(string code, string label)
    {
        return new MeritCycleMilestone(code, label, "", false, 0.0, 0, 0, "", MeritCycleMilestoneStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleMilestoneStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleMilestoneStatus next)
    {
        switch (Status)
        {
            case MeritCycleMilestoneStatus.Requested:
                switch (next)
                {
                    case MeritCycleMilestoneStatus.Granted:
                        return true;
                }
                return false;
            case MeritCycleMilestoneStatus.Granted:
                return next == MeritCycleMilestoneStatus.Revoked;
            case MeritCycleMilestoneStatus.Revoked:
                return next == MeritCycleMilestoneStatus.Expired;
        }
        return false;
    }

    public void MoveTo(MeritCycleMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Revision * (double)BatchSize, 2);
    }

    public MeritCycleMilestone Copy()
    {
        return new MeritCycleMilestone(
            Code,
            Label,
            Summary,
            Visible,
            Threshold,
            BatchSize,
            Revision,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
