namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped nomination assignment is in its life cycle.</summary>
public enum CappedNominationAssignmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A capped nomination assignment as the nomination team keeps it.</summary>
public sealed class CappedNominationAssignment(string code, string name, string country, double ratio, bool archived, int capacity, int durationMinutes, int graceDays, string externalRef, CappedNominationAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public double Ratio { get; set; } = ratio;
    public bool Archived { get; set; } = archived;
    public int Capacity { get; set; } = capacity;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int GraceDays { get; set; } = graceDays;
    public string ExternalRef { get; set; } = externalRef;
    public CappedNominationAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a capped nomination assignment in status new.</summary>
    public static CappedNominationAssignment NewCappedNominationAssignment(string code, string name)
    {
        return new CappedNominationAssignment(
            code,
            name,
            "",
            0.0,
            false,
            0,
            0,
            0,
            "",
            CappedNominationAssignmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedNominationAssignmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedNominationAssignmentStatus next)
    {
        switch (Status)
        {
            case CappedNominationAssignmentStatus.New:
                return next == CappedNominationAssignmentStatus.Failed;
            case CappedNominationAssignmentStatus.Queued:
                return next == CappedNominationAssignmentStatus.Failed;
            case CappedNominationAssignmentStatus.Running:
                switch (next)
                {
                    case CappedNominationAssignmentStatus.Done:
                        return true;
                }
                return false;
            case CappedNominationAssignmentStatus.Done:
                switch (next)
                {
                    case CappedNominationAssignmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedNominationAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped nomination assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedValue()
    {
        return Math.Round(Ratio - (double)DurationMinutes, 2);
    }

    public CappedNominationAssignment Copy()
    {
        return new CappedNominationAssignment(
            Code,
            Name,
            Country,
            Ratio,
            Archived,
            Capacity,
            DurationMinutes,
            GraceDays,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
