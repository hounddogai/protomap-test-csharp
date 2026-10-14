namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy exception profile is in its life cycle.</summary>
public enum LegacyExceptionProfileStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A legacy exception profile as the exception team keeps it.</summary>
public sealed class LegacyExceptionProfile(string code, string label, string notes, int endsAt, double ceiling, bool archived, LegacyExceptionProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public int EndsAt { get; set; } = endsAt;
    public double Ceiling { get; set; } = ceiling;
    public bool Archived { get; set; } = archived;
    public LegacyExceptionProfileStatus Status { get; set; } = status;

    /// <summary>Starts a legacy exception profile in status requested.</summary>
    public static LegacyExceptionProfile NewLegacyExceptionProfile(string code, string label)
    {
        return new LegacyExceptionProfile(code, label, "", 0, 0.0, false, LegacyExceptionProfileStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyExceptionProfileStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyExceptionProfileStatus next)
    {
        switch (Status)
        {
            case LegacyExceptionProfileStatus.Requested:
                switch (next)
                {
                    case LegacyExceptionProfileStatus.Revoked:
                    case LegacyExceptionProfileStatus.Expired:
                        return true;
                }
                return false;
            case LegacyExceptionProfileStatus.Granted:
                switch (next)
                {
                    case LegacyExceptionProfileStatus.Revoked:
                    case LegacyExceptionProfileStatus.Expired:
                        return true;
                }
                return false;
            case LegacyExceptionProfileStatus.Revoked:
                return next == LegacyExceptionProfileStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LegacyExceptionProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy exception profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)EndsAt * Ceiling, 2);
    }

    public LegacyExceptionProfile Copy()
    {
        return new LegacyExceptionProfile(Code, Label, Notes, EndsAt, Ceiling, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
