namespace Acme.Workforce.Talent.CareerPath.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy career path entry is in its life cycle.</summary>
public enum LegacyCareerPathEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A legacy career path entry as the career path team keeps it.</summary>
public sealed class LegacyCareerPathEntry(string code, string name, double fte, string skill, string segment, int quantity, string timezone, LegacyCareerPathEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public string Skill { get; set; } = skill;
    public string Segment { get; set; } = segment;
    public int Quantity { get; set; } = quantity;
    public string Timezone { get; set; } = timezone;
    public LegacyCareerPathEntryStatus Status { get; set; } = status;

    /// <summary>Starts a legacy career path entry in status new.</summary>
    public static LegacyCareerPathEntry NewLegacyCareerPathEntry(string code, string name)
    {
        return new LegacyCareerPathEntry(code, name, 0.0, "", "", 0, "", LegacyCareerPathEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyCareerPathEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyCareerPathEntryStatus next)
    {
        switch (Status)
        {
            case LegacyCareerPathEntryStatus.New:
                switch (next)
                {
                    case LegacyCareerPathEntryStatus.Queued:
                        return true;
                }
                return false;
            case LegacyCareerPathEntryStatus.Queued:
                switch (next)
                {
                    case LegacyCareerPathEntryStatus.Running:
                    case LegacyCareerPathEntryStatus.Done:
                        return true;
                }
                return false;
            case LegacyCareerPathEntryStatus.Running:
                return next == LegacyCareerPathEntryStatus.Done || next == LegacyCareerPathEntryStatus.Failed;
            case LegacyCareerPathEntryStatus.Done:
                switch (next)
                {
                    case LegacyCareerPathEntryStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyCareerPathEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy career path entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Quantity - Fte, 2);
    }

    public LegacyCareerPathEntry Copy()
    {
        return new LegacyCareerPathEntry(Code, Name, Fte, Skill, Segment, Quantity, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
