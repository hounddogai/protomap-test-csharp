namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global attestation group is in its life cycle.</summary>
public enum GlobalAttestationGroupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One global attestation group, with what the planners need to act on it.</summary>
public sealed class GlobalAttestationGroup(string code, string name, int priority, double hours, string team, GlobalAttestationGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public double Hours { get; set; } = hours;
    public string Team { get; set; } = team;
    public GlobalAttestationGroupStatus Status { get; set; } = status;

    /// <summary>Starts a global attestation group in status new.</summary>
    public static GlobalAttestationGroup NewGlobalAttestationGroup(string code, string name)
    {
        return new GlobalAttestationGroup(code, name, 0, 0.0, "", GlobalAttestationGroupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAttestationGroupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAttestationGroupStatus next)
    {
        switch (Status)
        {
            case GlobalAttestationGroupStatus.New:
                return next == GlobalAttestationGroupStatus.Running || next == GlobalAttestationGroupStatus.Done;
            case GlobalAttestationGroupStatus.Queued:
                return next == GlobalAttestationGroupStatus.Done;
            case GlobalAttestationGroupStatus.Running:
                switch (next)
                {
                    case GlobalAttestationGroupStatus.Done:
                    case GlobalAttestationGroupStatus.Failed:
                        return true;
                }
                return false;
            case GlobalAttestationGroupStatus.Done:
                return next == GlobalAttestationGroupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(GlobalAttestationGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global attestation group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Priority + Hours, 2);
    }

    public GlobalAttestationGroup Copy()
    {
        return new GlobalAttestationGroup(Code, Name, Priority, Hours, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
