namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination preference is in its life cycle.</summary>
public enum NominationPreferenceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One nomination preference, with what the planners need to act on it.</summary>
public sealed class NominationPreference(string code, string name, string category, int version, int leaveDays, NominationPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public int Version { get; set; } = version;
    public int LeaveDays { get; set; } = leaveDays;
    public NominationPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a nomination preference in status idle.</summary>
    public static NominationPreference NewNominationPreference(string code, string name)
    {
        return new NominationPreference(code, name, "", 0, 0, NominationPreferenceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationPreferenceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationPreferenceStatus next)
    {
        switch (Status)
        {
            case NominationPreferenceStatus.Idle:
                switch (next)
                {
                    case NominationPreferenceStatus.Stopped:
                        return true;
                }
                return false;
            case NominationPreferenceStatus.Warming:
                switch (next)
                {
                    case NominationPreferenceStatus.Draining:
                    case NominationPreferenceStatus.Stopped:
                        return true;
                }
                return false;
            case NominationPreferenceStatus.Ready:
                return next == NominationPreferenceStatus.Draining || next == NominationPreferenceStatus.Stopped;
            case NominationPreferenceStatus.Draining:
                switch (next)
                {
                    case NominationPreferenceStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedValue()
    {
        return Math.Round((double)LeaveDays * (double)Version, 2);
    }

    public NominationPreference Copy()
    {
        return new NominationPreference(Code, Name, Category, Version, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
