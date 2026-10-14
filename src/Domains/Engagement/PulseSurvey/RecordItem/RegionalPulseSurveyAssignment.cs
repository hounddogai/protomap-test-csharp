namespace Acme.Workforce.Engagement.PulseSurvey.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional pulse survey assignment is in its life cycle.</summary>
public enum RegionalPulseSurveyAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One regional pulse survey assignment, with what the planners need to act on it.</summary>
public sealed class RegionalPulseSurveyAssignment(string code, string label, double maximum, bool pinned, string currency, bool archived, int sequence, int retries, int batchSize, int revision, RegionalPulseSurveyAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public bool Pinned { get; set; } = pinned;
    public string Currency { get; set; } = currency;
    public bool Archived { get; set; } = archived;
    public int Sequence { get; set; } = sequence;
    public int Retries { get; set; } = retries;
    public int BatchSize { get; set; } = batchSize;
    public int Revision { get; set; } = revision;
    public RegionalPulseSurveyAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a regional pulse survey assignment in status proposed.</summary>
    public static RegionalPulseSurveyAssignment NewRegionalPulseSurveyAssignment(string code, string label)
    {
        return new RegionalPulseSurveyAssignment(
            code,
            label,
            0.0,
            false,
            "",
            false,
            0,
            0,
            0,
            0,
            RegionalPulseSurveyAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPulseSurveyAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPulseSurveyAssignmentStatus next)
    {
        switch (Status)
        {
            case RegionalPulseSurveyAssignmentStatus.Proposed:
                return next == RegionalPulseSurveyAssignmentStatus.Approved || next == RegionalPulseSurveyAssignmentStatus.Withdrawn;
            case RegionalPulseSurveyAssignmentStatus.Approved:
                return next == RegionalPulseSurveyAssignmentStatus.Withdrawn;
            case RegionalPulseSurveyAssignmentStatus.Rejected:
                return next == RegionalPulseSurveyAssignmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RegionalPulseSurveyAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional pulse survey assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Maximum * (double)BatchSize, 2);
    }

    public RegionalPulseSurveyAssignment Copy()
    {
        return new RegionalPulseSurveyAssignment(
            Code,
            Label,
            Maximum,
            Pinned,
            Currency,
            Archived,
            Sequence,
            Retries,
            BatchSize,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
