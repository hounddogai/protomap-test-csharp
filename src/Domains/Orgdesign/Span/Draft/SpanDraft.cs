namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span draft is in its life cycle.</summary>
public enum SpanDraftStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A span draft. Its code is unique within a tenant.</summary>
public sealed class SpanDraft(string code, string title, string currency, string jobCode, string region, int priority, string notes, string contactEmail, int batchSize, SpanDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public string JobCode { get; set; } = jobCode;
    public string Region { get; set; } = region;
    public int Priority { get; set; } = priority;
    public string Notes { get; set; } = notes;
    public string ContactEmail { get; set; } = contactEmail;
    public int BatchSize { get; set; } = batchSize;
    public SpanDraftStatus Status { get; set; } = status;

    /// <summary>Starts a span draft in status draft.</summary>
    public static SpanDraft NewSpanDraft(string code, string title)
    {
        return new SpanDraft(code, title, "", "", "", 0, "", "", 0, SpanDraftStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanDraftStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanDraftStatus next)
    {
        switch (Status)
        {
            case SpanDraftStatus.Draft:
                switch (next)
                {
                    case SpanDraftStatus.Pending:
                    case SpanDraftStatus.Closed:
                        return true;
                }
                return false;
            case SpanDraftStatus.Pending:
                switch (next)
                {
                    case SpanDraftStatus.Suspended:
                    case SpanDraftStatus.Closed:
                        return true;
                }
                return false;
            case SpanDraftStatus.Active:
                switch (next)
                {
                    case SpanDraftStatus.Suspended:
                        return true;
                }
                return false;
            case SpanDraftStatus.Suspended:
                switch (next)
                {
                    case SpanDraftStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"span draft"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Priority + (double)BatchSize, 2);
    }

    public SpanDraft Copy()
    {
        return new SpanDraft(
            Code,
            Title,
            Currency,
            JobCode,
            Region,
            Priority,
            Notes,
            ContactEmail,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
