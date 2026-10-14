namespace Acme.Workforce.Absence.LeaveRequest.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling leave request preference is in its life cycle.</summary>
public enum RollingLeaveRequestPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A rolling leave request preference. Its code is unique within a tenant.</summary>
public sealed class RollingLeaveRequestPreference(string code, string label, string region, string team, string phone, string timezone, double cost, double threshold, int windowDays, RollingLeaveRequestPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public string Team { get; set; } = team;
    public string Phone { get; set; } = phone;
    public string Timezone { get; set; } = timezone;
    public double Cost { get; set; } = cost;
    public double Threshold { get; set; } = threshold;
    public int WindowDays { get; set; } = windowDays;
    public RollingLeaveRequestPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a rolling leave request preference in status new.</summary>
    public static RollingLeaveRequestPreference NewRollingLeaveRequestPreference(string code, string label)
    {
        return new RollingLeaveRequestPreference(
            code,
            label,
            "",
            "",
            "",
            "",
            0.0,
            0.0,
            0,
            RollingLeaveRequestPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingLeaveRequestPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingLeaveRequestPreferenceStatus next)
    {
        switch (Status)
        {
            case RollingLeaveRequestPreferenceStatus.New:
                switch (next)
                {
                    case RollingLeaveRequestPreferenceStatus.Queued:
                    case RollingLeaveRequestPreferenceStatus.Failed:
                        return true;
                }
                return false;
            case RollingLeaveRequestPreferenceStatus.Queued:
                return next == RollingLeaveRequestPreferenceStatus.Done;
            case RollingLeaveRequestPreferenceStatus.Running:
                switch (next)
                {
                    case RollingLeaveRequestPreferenceStatus.Done:
                        return true;
                }
                return false;
            case RollingLeaveRequestPreferenceStatus.Done:
                switch (next)
                {
                    case RollingLeaveRequestPreferenceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingLeaveRequestPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling leave request preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling leave request preference"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)WindowDays - Cost, 2);
    }

    public RollingLeaveRequestPreference Copy()
    {
        return new RollingLeaveRequestPreference(
            Code,
            Label,
            Region,
            Team,
            Phone,
            Timezone,
            Cost,
            Threshold,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
