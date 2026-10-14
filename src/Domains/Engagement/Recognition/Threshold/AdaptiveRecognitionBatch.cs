namespace Acme.Workforce.Engagement.Recognition.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive recognition batch is in its life cycle.</summary>
public enum AdaptiveRecognitionBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A adaptive recognition batch as the recognition team keeps it.</summary>
public sealed class AdaptiveRecognitionBatch(string code, string label, double margin, double maximum, string location, string notes, string description, string team, double budget, AdaptiveRecognitionBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public double Maximum { get; set; } = maximum;
    public string Location { get; set; } = location;
    public string Notes { get; set; } = notes;
    public string Description { get; set; } = description;
    public string Team { get; set; } = team;
    public double Budget { get; set; } = budget;
    public AdaptiveRecognitionBatchStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive recognition batch in status created.</summary>
    public static AdaptiveRecognitionBatch NewAdaptiveRecognitionBatch(string code, string label)
    {
        return new AdaptiveRecognitionBatch(
            code,
            label,
            0.0,
            0.0,
            "",
            "",
            "",
            "",
            0.0,
            AdaptiveRecognitionBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveRecognitionBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveRecognitionBatchStatus next)
    {
        switch (Status)
        {
            case AdaptiveRecognitionBatchStatus.Created:
                return next == AdaptiveRecognitionBatchStatus.Published;
            case AdaptiveRecognitionBatchStatus.Validated:
                switch (next)
                {
                    case AdaptiveRecognitionBatchStatus.Retired:
                        return true;
                }
                return false;
            case AdaptiveRecognitionBatchStatus.Published:
                return next == AdaptiveRecognitionBatchStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdaptiveRecognitionBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive recognition batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adaptive recognition batch"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Budget - Maximum, 2);
    }

    public AdaptiveRecognitionBatch Copy()
    {
        return new AdaptiveRecognitionBatch(
            Code,
            Label,
            Margin,
            Maximum,
            Location,
            Notes,
            Description,
            Team,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
