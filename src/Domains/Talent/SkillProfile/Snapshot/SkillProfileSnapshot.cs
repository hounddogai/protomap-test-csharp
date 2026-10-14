namespace Acme.Workforce.Talent.SkillProfile.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile snapshot is in its life cycle.</summary>
public enum SkillProfileSnapshotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a skill profile snapshot and its current status.</summary>
public sealed class SkillProfileSnapshot(string code, string label, double floorValue, string jobCode, double minimum, SkillProfileSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public string JobCode { get; set; } = jobCode;
    public double Minimum { get; set; } = minimum;
    public SkillProfileSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile snapshot in status open.</summary>
    public static SkillProfileSnapshot NewSkillProfileSnapshot(string code, string label)
    {
        return new SkillProfileSnapshot(code, label, 0.0, "", 0.0, SkillProfileSnapshotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileSnapshotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileSnapshotStatus next)
    {
        switch (Status)
        {
            case SkillProfileSnapshotStatus.Open:
                return next == SkillProfileSnapshotStatus.Reopened;
            case SkillProfileSnapshotStatus.InReview:
                switch (next)
                {
                    case SkillProfileSnapshotStatus.Resolved:
                    case SkillProfileSnapshotStatus.Archived:
                        return true;
                }
                return false;
            case SkillProfileSnapshotStatus.Resolved:
                return next == SkillProfileSnapshotStatus.Reopened || next == SkillProfileSnapshotStatus.Archived;
            case SkillProfileSnapshotStatus.Reopened:
                switch (next)
                {
                    case SkillProfileSnapshotStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Minimum * FloorValue, 2);
    }

    public SkillProfileSnapshot Copy()
    {
        return new SkillProfileSnapshot(Code, Label, FloorValue, JobCode, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
