namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span bundle is in its life cycle.</summary>
public enum SpanBundleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A span bundle as the span team keeps it.</summary>
public sealed class SpanBundle(string code, string name, int endsAt, int expiresAt, string phone, SpanBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Phone { get; set; } = phone;
    public SpanBundleStatus Status { get; set; } = status;

    /// <summary>Starts a span bundle in status open.</summary>
    public static SpanBundle NewSpanBundle(string code, string name)
    {
        return new SpanBundle(code, name, 0, 0, "", SpanBundleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanBundleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanBundleStatus next)
    {
        switch (Status)
        {
            case SpanBundleStatus.Open:
                switch (next)
                {
                    case SpanBundleStatus.Resolved:
                    case SpanBundleStatus.Reopened:
                        return true;
                }
                return false;
            case SpanBundleStatus.InReview:
                switch (next)
                {
                    case SpanBundleStatus.Resolved:
                    case SpanBundleStatus.Archived:
                        return true;
                }
                return false;
            case SpanBundleStatus.Resolved:
                return next == SpanBundleStatus.Reopened;
            case SpanBundleStatus.Reopened:
                return next == SpanBundleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(SpanBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"span bundle"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)EndsAt - (double)ExpiresAt, 2);
    }

    public SpanBundle Copy()
    {
        return new SpanBundle(Code, Name, EndsAt, ExpiresAt, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
