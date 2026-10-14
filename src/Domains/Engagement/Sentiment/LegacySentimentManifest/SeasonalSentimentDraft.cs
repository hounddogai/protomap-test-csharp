namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal sentiment draft is in its life cycle.</summary>
public enum SeasonalSentimentDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A seasonal sentiment draft. Its code is unique within a tenant.</summary>
public sealed class SeasonalSentimentDraft(string code, string label, string externalRef, string checksum, bool visible, int quantity, SeasonalSentimentDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string Checksum { get; set; } = checksum;
    public bool Visible { get; set; } = visible;
    public int Quantity { get; set; } = quantity;
    public SeasonalSentimentDraftStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal sentiment draft in status planned.</summary>
    public static SeasonalSentimentDraft NewSeasonalSentimentDraft(string code, string label)
    {
        return new SeasonalSentimentDraft(code, label, "", "", false, 0, SeasonalSentimentDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalSentimentDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalSentimentDraftStatus next)
    {
        switch (Status)
        {
            case SeasonalSentimentDraftStatus.Planned:
                return next == SeasonalSentimentDraftStatus.Cancelled;
            case SeasonalSentimentDraftStatus.Scheduled:
                return next == SeasonalSentimentDraftStatus.InProgress || next == SeasonalSentimentDraftStatus.Cancelled;
            case SeasonalSentimentDraftStatus.InProgress:
                switch (next)
                {
                    case SeasonalSentimentDraftStatus.Completed:
                    case SeasonalSentimentDraftStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalSentimentDraftStatus.Completed:
                switch (next)
                {
                    case SeasonalSentimentDraftStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalSentimentDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal sentiment draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"seasonal sentiment draft"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public SeasonalSentimentDraft Copy()
    {
        return new SeasonalSentimentDraft(Code, Label, ExternalRef, Checksum, Visible, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
