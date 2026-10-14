namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat preference is in its life cycle.</summary>
public enum LicenseSeatPreferenceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a license seat preference and its current status.</summary>
public sealed class LicenseSeatPreference(string code, string title, string description, string checksum, double hours, double fte, int version, bool visible, LicenseSeatPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public string Checksum { get; set; } = checksum;
    public double Hours { get; set; } = hours;
    public double Fte { get; set; } = fte;
    public int Version { get; set; } = version;
    public bool Visible { get; set; } = visible;
    public LicenseSeatPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a license seat preference in status planned.</summary>
    public static LicenseSeatPreference NewLicenseSeatPreference(string code, string title)
    {
        return new LicenseSeatPreference(
            code,
            title,
            "",
            "",
            0.0,
            0.0,
            0,
            false,
            LicenseSeatPreferenceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatPreferenceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatPreferenceStatus next)
    {
        switch (Status)
        {
            case LicenseSeatPreferenceStatus.Planned:
                switch (next)
                {
                    case LicenseSeatPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
            case LicenseSeatPreferenceStatus.Scheduled:
                return next == LicenseSeatPreferenceStatus.Completed || next == LicenseSeatPreferenceStatus.Cancelled;
            case LicenseSeatPreferenceStatus.InProgress:
                return next == LicenseSeatPreferenceStatus.Cancelled;
            case LicenseSeatPreferenceStatus.Completed:
                return next == LicenseSeatPreferenceStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LicenseSeatPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetTotal()
    {
        return Math.Round(Hours * Fte, 2);
    }

    public LicenseSeatPreference Copy()
    {
        return new LicenseSeatPreference(Code, Title, Description, Checksum, Hours, Fte, Version, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
