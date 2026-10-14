namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span manifest is in its life cycle.</summary>
public enum SpanManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a span manifest and its current status.</summary>
public sealed class SpanManifest(string code, string title, string jobCode, double fte, double rating, string label, bool visible, int batchSize, string region, int leaveDays, SpanManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public double Fte { get; set; } = fte;
    public double Rating { get; set; } = rating;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public int BatchSize { get; set; } = batchSize;
    public string Region { get; set; } = region;
    public int LeaveDays { get; set; } = leaveDays;
    public SpanManifestStatus Status { get; set; } = status;

    /// <summary>Starts a span manifest in status planned.</summary>
    public static SpanManifest NewSpanManifest(string code, string title)
    {
        return new SpanManifest(code, title, "", 0.0, 0.0, "", false, 0, "", 0, SpanManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanManifestStatus next)
    {
        switch (Status)
        {
            case SpanManifestStatus.Planned:
                return next == SpanManifestStatus.Completed;
            case SpanManifestStatus.Scheduled:
                return next == SpanManifestStatus.Cancelled;
            case SpanManifestStatus.InProgress:
                switch (next)
                {
                    case SpanManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case SpanManifestStatus.Completed:
                switch (next)
                {
                    case SpanManifestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)BatchSize - Fte, 2);
    }

    public SpanManifest Copy()
    {
        return new SpanManifest(
            Code,
            Title,
            JobCode,
            Fte,
            Rating,
            Label,
            Visible,
            BatchSize,
            Region,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
