namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy leave request preference is in its life cycle.</summary>
public enum LegacyLeaveRequestPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One legacy leave request preference, with what the planners need to act on it.</summary>
public sealed class LegacyLeaveRequestPreference(string code, string label, double minimum, double threshold, string title, string country, int windowDays, LegacyLeaveRequestPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public double Threshold { get; set; } = threshold;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public int WindowDays { get; set; } = windowDays;
    public LegacyLeaveRequestPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a legacy leave request preference in status created.</summary>
    public static LegacyLeaveRequestPreference NewLegacyLeaveRequestPreference(string code, string label)
    {
        return new LegacyLeaveRequestPreference(
            code,
            label,
            0.0,
            0.0,
            "",
            "",
            0,
            LegacyLeaveRequestPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyLeaveRequestPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyLeaveRequestPreferenceStatus next)
    {
        switch (Status)
        {
            case LegacyLeaveRequestPreferenceStatus.Created:
                switch (next)
                {
                    case LegacyLeaveRequestPreferenceStatus.Validated:
                    case LegacyLeaveRequestPreferenceStatus.Retired:
                        return true;
                }
                return false;
            case LegacyLeaveRequestPreferenceStatus.Validated:
                return next == LegacyLeaveRequestPreferenceStatus.Published || next == LegacyLeaveRequestPreferenceStatus.Retired;
            case LegacyLeaveRequestPreferenceStatus.Published:
                return next == LegacyLeaveRequestPreferenceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LegacyLeaveRequestPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy leave request preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetLoad()
    {
        return Math.Round(Threshold - Minimum, 2);
    }

    public LegacyLeaveRequestPreference Copy()
    {
        return new LegacyLeaveRequestPreference(Code, Label, Minimum, Threshold, Title, Country, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
