namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile segment is in its life cycle.</summary>
public enum SkillProfileSegmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One skill profile segment, with what the planners need to act on it.</summary>
public sealed class SkillProfileSegment(string code, string label, double ratio, int revision, double ceiling, string costCenter, int batchSize, SkillProfileSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public int Revision { get; set; } = revision;
    public double Ceiling { get; set; } = ceiling;
    public string CostCenter { get; set; } = costCenter;
    public int BatchSize { get; set; } = batchSize;
    public SkillProfileSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile segment in status created.</summary>
    public static SkillProfileSegment NewSkillProfileSegment(string code, string label)
    {
        return new SkillProfileSegment(code, label, 0.0, 0, 0.0, "", 0, SkillProfileSegmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileSegmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileSegmentStatus next)
    {
        switch (Status)
        {
            case SkillProfileSegmentStatus.Created:
                switch (next)
                {
                    case SkillProfileSegmentStatus.Published:
                    case SkillProfileSegmentStatus.Retired:
                        return true;
                }
                return false;
            case SkillProfileSegmentStatus.Validated:
                switch (next)
                {
                    case SkillProfileSegmentStatus.Published:
                    case SkillProfileSegmentStatus.Retired:
                        return true;
                }
                return false;
            case SkillProfileSegmentStatus.Published:
                return next == SkillProfileSegmentStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SkillProfileSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Ratio + (double)Revision, 2);
    }

    public SkillProfileSegment Copy()
    {
        return new SkillProfileSegment(Code, Label, Ratio, Revision, Ceiling, CostCenter, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
