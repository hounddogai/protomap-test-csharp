namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile reservation is in its life cycle.</summary>
public enum SkillProfileReservationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a skill profile reservation and its current status.</summary>
public sealed class SkillProfileReservation(string code, string title, int retries, int version, string notes, SkillProfileReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public int Version { get; set; } = version;
    public string Notes { get; set; } = notes;
    public SkillProfileReservationStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile reservation in status planned.</summary>
    public static SkillProfileReservation NewSkillProfileReservation(string code, string title)
    {
        return new SkillProfileReservation(code, title, 0, 0, "", SkillProfileReservationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileReservationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileReservationStatus next)
    {
        switch (Status)
        {
            case SkillProfileReservationStatus.Planned:
                return next == SkillProfileReservationStatus.Cancelled;
            case SkillProfileReservationStatus.Scheduled:
                return next == SkillProfileReservationStatus.Cancelled;
            case SkillProfileReservationStatus.InProgress:
                switch (next)
                {
                    case SkillProfileReservationStatus.Completed:
                        return true;
                }
                return false;
            case SkillProfileReservationStatus.Completed:
                switch (next)
                {
                    case SkillProfileReservationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Retries * (double)Version, 2);
    }

    public SkillProfileReservation Copy()
    {
        return new SkillProfileReservation(Code, Title, Retries, Version, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
