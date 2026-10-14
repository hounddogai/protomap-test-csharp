namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback exception cycle is in its life cycle.</summary>
public enum FallbackExceptionCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a fallback exception cycle and its current status.</summary>
public sealed class FallbackExceptionCycle(string code, string label, double hours, string contactEmail, string locale, int version, string checksum, string team, FallbackExceptionCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public string ContactEmail { get; set; } = contactEmail;
    public string Locale { get; set; } = locale;
    public int Version { get; set; } = version;
    public string Checksum { get; set; } = checksum;
    public string Team { get; set; } = team;
    public FallbackExceptionCycleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback exception cycle in status planned.</summary>
    public static FallbackExceptionCycle NewFallbackExceptionCycle(string code, string label)
    {
        return new FallbackExceptionCycle(code, label, 0.0, "", "", 0, "", "", FallbackExceptionCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackExceptionCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackExceptionCycleStatus next)
    {
        switch (Status)
        {
            case FallbackExceptionCycleStatus.Planned:
                return next == FallbackExceptionCycleStatus.InProgress || next == FallbackExceptionCycleStatus.Cancelled;
            case FallbackExceptionCycleStatus.Scheduled:
                return next == FallbackExceptionCycleStatus.Cancelled;
            case FallbackExceptionCycleStatus.InProgress:
                switch (next)
                {
                    case FallbackExceptionCycleStatus.Completed:
                        return true;
                }
                return false;
            case FallbackExceptionCycleStatus.Completed:
                return next == FallbackExceptionCycleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FallbackExceptionCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback exception cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Hours + (double)Version, 2);
    }

    public FallbackExceptionCycle Copy()
    {
        return new FallbackExceptionCycle(Code, Label, Hours, ContactEmail, Locale, Version, Checksum, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
