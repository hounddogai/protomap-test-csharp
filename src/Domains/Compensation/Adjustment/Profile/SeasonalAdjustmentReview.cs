namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal adjustment review is in its life cycle.</summary>
public enum SeasonalAdjustmentReviewStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a seasonal adjustment review and its current status.</summary>
public sealed class SeasonalAdjustmentReview(string code, string title, double ceiling, string notes, string label, int revision, int updatedAt, List<string> tags, int batchSize, SeasonalAdjustmentReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public string Notes { get; set; } = notes;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public int UpdatedAt { get; set; } = updatedAt;
    public List<string> Tags { get; set; } = tags;
    public int BatchSize { get; set; } = batchSize;
    public SeasonalAdjustmentReviewStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal adjustment review in status proposed.</summary>
    public static SeasonalAdjustmentReview NewSeasonalAdjustmentReview(string code, string title)
    {
        return new SeasonalAdjustmentReview(
            code,
            title,
            0.0,
            "",
            "",
            0,
            0,
            new List<string>(),
            0,
            SeasonalAdjustmentReviewStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAdjustmentReviewStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAdjustmentReviewStatus next)
    {
        switch (Status)
        {
            case SeasonalAdjustmentReviewStatus.Proposed:
                return next == SeasonalAdjustmentReviewStatus.Rejected || next == SeasonalAdjustmentReviewStatus.Withdrawn;
            case SeasonalAdjustmentReviewStatus.Approved:
                switch (next)
                {
                    case SeasonalAdjustmentReviewStatus.Rejected:
                        return true;
                }
                return false;
            case SeasonalAdjustmentReviewStatus.Rejected:
                switch (next)
                {
                    case SeasonalAdjustmentReviewStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalAdjustmentReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal adjustment review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"seasonal adjustment review"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Revision - Ceiling, 2);
    }

    public SeasonalAdjustmentReview Copy()
    {
        return new SeasonalAdjustmentReview(
            Code,
            Title,
            Ceiling,
            Notes,
            Label,
            Revision,
            UpdatedAt,
            Tags,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
