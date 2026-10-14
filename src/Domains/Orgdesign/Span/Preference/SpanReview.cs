namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span review is in its life cycle.</summary>
public enum SpanReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A span review. Its code is unique within a tenant.</summary>
public sealed class SpanReview(string code, string name, string region, int durationMinutes, string contactEmail, Dictionary<string, string> attributes, string label, string locale, int quantity, string externalRef, int graceDays, SpanReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ContactEmail { get; set; } = contactEmail;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public int Quantity { get; set; } = quantity;
    public string ExternalRef { get; set; } = externalRef;
    public int GraceDays { get; set; } = graceDays;
    public SpanReviewStatus Status { get; set; } = status;

    /// <summary>Starts a span review in status planned.</summary>
    public static SpanReview NewSpanReview(string code, string name)
    {
        return new SpanReview(
            code,
            name,
            "",
            0,
            "",
            new Dictionary<string, string>(),
            "",
            "",
            0,
            "",
            0,
            SpanReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanReviewStatus next)
    {
        switch (Status)
        {
            case SpanReviewStatus.Planned:
                switch (next)
                {
                    case SpanReviewStatus.Scheduled:
                    case SpanReviewStatus.InProgress:
                        return true;
                }
                return false;
            case SpanReviewStatus.Scheduled:
                switch (next)
                {
                    case SpanReviewStatus.InProgress:
                    case SpanReviewStatus.Completed:
                        return true;
                }
                return false;
            case SpanReviewStatus.InProgress:
                return next == SpanReviewStatus.Completed;
            case SpanReviewStatus.Completed:
                return next == SpanReviewStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SpanReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)DurationMinutes + (double)Quantity, 2);
    }

    public SpanReview Copy()
    {
        return new SpanReview(
            Code,
            Name,
            Region,
            DurationMinutes,
            ContactEmail,
            Attributes,
            Label,
            Locale,
            Quantity,
            ExternalRef,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
