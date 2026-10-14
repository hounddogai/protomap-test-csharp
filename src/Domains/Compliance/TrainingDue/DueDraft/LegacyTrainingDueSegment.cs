namespace Acme.Workforce.Compliance.TrainingDue.DueDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy training due segment is in its life cycle.</summary>
public enum LegacyTrainingDueSegmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One legacy training due segment, with what the planners need to act on it.</summary>
public sealed class LegacyTrainingDueSegment(string code, string label, int revision, int quantity, string checksum, bool active, double budget, LegacyTrainingDueSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public int Quantity { get; set; } = quantity;
    public string Checksum { get; set; } = checksum;
    public bool Active { get; set; } = active;
    public double Budget { get; set; } = budget;
    public LegacyTrainingDueSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a legacy training due segment in status created.</summary>
    public static LegacyTrainingDueSegment NewLegacyTrainingDueSegment(string code, string label)
    {
        return new LegacyTrainingDueSegment(
            code,
            label,
            0,
            0,
            "",
            false,
            0.0,
            LegacyTrainingDueSegmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTrainingDueSegmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTrainingDueSegmentStatus next)
    {
        switch (Status)
        {
            case LegacyTrainingDueSegmentStatus.Created:
                return next == LegacyTrainingDueSegmentStatus.Published || next == LegacyTrainingDueSegmentStatus.Retired;
            case LegacyTrainingDueSegmentStatus.Validated:
                return next == LegacyTrainingDueSegmentStatus.Published;
            case LegacyTrainingDueSegmentStatus.Published:
                return next == LegacyTrainingDueSegmentStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LegacyTrainingDueSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy training due segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Budget + (double)Revision, 2);
    }

    public LegacyTrainingDueSegment Copy()
    {
        return new LegacyTrainingDueSegment(Code, Label, Revision, Quantity, Checksum, Active, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
