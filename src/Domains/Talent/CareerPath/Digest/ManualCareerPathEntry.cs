namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual career path entry is in its life cycle.</summary>
public enum ManualCareerPathEntryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual career path entry. Its code is unique within a tenant.</summary>
public sealed class ManualCareerPathEntry(string code, string label, string skill, double maximum, int capacity, ManualCareerPathEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public double Maximum { get; set; } = maximum;
    public int Capacity { get; set; } = capacity;
    public ManualCareerPathEntryStatus Status { get; set; } = status;

    /// <summary>Starts a manual career path entry in status created.</summary>
    public static ManualCareerPathEntry NewManualCareerPathEntry(string code, string label)
    {
        return new ManualCareerPathEntry(code, label, "", 0.0, 0, ManualCareerPathEntryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualCareerPathEntryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualCareerPathEntryStatus next)
    {
        switch (Status)
        {
            case ManualCareerPathEntryStatus.Created:
                return next == ManualCareerPathEntryStatus.Retired;
            case ManualCareerPathEntryStatus.Validated:
                return next == ManualCareerPathEntryStatus.Published;
            case ManualCareerPathEntryStatus.Published:
                switch (next)
                {
                    case ManualCareerPathEntryStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualCareerPathEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual career path entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Capacity * Maximum, 2);
    }

    public ManualCareerPathEntry Copy()
    {
        return new ManualCareerPathEntry(Code, Label, Skill, Maximum, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
