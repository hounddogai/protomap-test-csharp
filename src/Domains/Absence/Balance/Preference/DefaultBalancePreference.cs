namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default balance preference is in its life cycle.</summary>
public enum DefaultBalancePreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One default balance preference, with what the planners need to act on it.</summary>
public sealed class DefaultBalancePreference(string code, string label, int leaveDays, double cost, string name, string ownerId, DefaultBalancePreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public double Cost { get; set; } = cost;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public DefaultBalancePreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a default balance preference in status new.</summary>
    public static DefaultBalancePreference NewDefaultBalancePreference(string code, string label)
    {
        return new DefaultBalancePreference(code, label, 0, 0.0, "", "", DefaultBalancePreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultBalancePreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultBalancePreferenceStatus next)
    {
        switch (Status)
        {
            case DefaultBalancePreferenceStatus.New:
                return next == DefaultBalancePreferenceStatus.Running;
            case DefaultBalancePreferenceStatus.Queued:
                switch (next)
                {
                    case DefaultBalancePreferenceStatus.Running:
                        return true;
                }
                return false;
            case DefaultBalancePreferenceStatus.Running:
                return next == DefaultBalancePreferenceStatus.Done;
            case DefaultBalancePreferenceStatus.Done:
                switch (next)
                {
                    case DefaultBalancePreferenceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultBalancePreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default balance preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"default balance preference"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)LeaveDays * Cost, 2);
    }

    public DefaultBalancePreference Copy()
    {
        return new DefaultBalancePreference(Code, Label, LeaveDays, Cost, Name, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
