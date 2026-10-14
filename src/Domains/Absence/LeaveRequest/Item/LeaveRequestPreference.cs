namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request preference is in its life cycle.</summary>
public enum LeaveRequestPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A leave request preference as the leave request team keeps it.</summary>
public sealed class LeaveRequestPreference(string code, string name, string externalRef, bool archived, string skill, int createdAt, int leaveDays, int startsAt, LeaveRequestPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public bool Archived { get; set; } = archived;
    public string Skill { get; set; } = skill;
    public int CreatedAt { get; set; } = createdAt;
    public int LeaveDays { get; set; } = leaveDays;
    public int StartsAt { get; set; } = startsAt;
    public LeaveRequestPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a leave request preference in status created.</summary>
    public static LeaveRequestPreference NewLeaveRequestPreference(string code, string name)
    {
        return new LeaveRequestPreference(code, name, "", false, "", 0, 0, 0, LeaveRequestPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestPreferenceStatus next)
    {
        switch (Status)
        {
            case LeaveRequestPreferenceStatus.Created:
                switch (next)
                {
                    case LeaveRequestPreferenceStatus.Validated:
                    case LeaveRequestPreferenceStatus.Retired:
                        return true;
                }
                return false;
            case LeaveRequestPreferenceStatus.Validated:
                return next == LeaveRequestPreferenceStatus.Published;
            case LeaveRequestPreferenceStatus.Published:
                return next == LeaveRequestPreferenceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LeaveRequestPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)CreatedAt * (double)LeaveDays, 2);
    }

    public LeaveRequestPreference Copy()
    {
        return new LeaveRequestPreference(
            Code,
            Name,
            ExternalRef,
            Archived,
            Skill,
            CreatedAt,
            LeaveDays,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
