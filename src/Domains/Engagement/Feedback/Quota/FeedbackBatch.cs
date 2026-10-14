namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback batch is in its life cycle.</summary>
public enum FeedbackBatchStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A feedback batch. Its code is unique within a tenant.</summary>
public sealed class FeedbackBatch(string code, string label, string country, string externalRef, string checksum, string skill, int revision, int durationMinutes, string ownerId, string notes, FeedbackBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public string ExternalRef { get; set; } = externalRef;
    public string Checksum { get; set; } = checksum;
    public string Skill { get; set; } = skill;
    public int Revision { get; set; } = revision;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string OwnerId { get; set; } = ownerId;
    public string Notes { get; set; } = notes;
    public FeedbackBatchStatus Status { get; set; } = status;

    /// <summary>Starts a feedback batch in status proposed.</summary>
    public static FeedbackBatch NewFeedbackBatch(string code, string label)
    {
        return new FeedbackBatch(code, label, "", "", "", "", 0, 0, "", "", FeedbackBatchStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackBatchStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackBatchStatus next)
    {
        switch (Status)
        {
            case FeedbackBatchStatus.Proposed:
                return next == FeedbackBatchStatus.Withdrawn;
            case FeedbackBatchStatus.Approved:
                return next == FeedbackBatchStatus.Rejected || next == FeedbackBatchStatus.Withdrawn;
            case FeedbackBatchStatus.Rejected:
                return next == FeedbackBatchStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FeedbackBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"feedback batch"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)DurationMinutes - (double)Revision, 2);
    }

    public FeedbackBatch Copy()
    {
        return new FeedbackBatch(
            Code,
            Label,
            Country,
            ExternalRef,
            Checksum,
            Skill,
            Revision,
            DurationMinutes,
            OwnerId,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
