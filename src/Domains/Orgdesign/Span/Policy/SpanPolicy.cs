namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span policy is in its life cycle.</summary>
public enum SpanPolicyStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A span policy as the span team keeps it.</summary>
public sealed class SpanPolicy(string code, string title, double maximum, int leaveDays, int updatedAt, int attempts, string source, string location, double margin, SpanPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public int LeaveDays { get; set; } = leaveDays;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Attempts { get; set; } = attempts;
    public string Source { get; set; } = source;
    public string Location { get; set; } = location;
    public double Margin { get; set; } = margin;
    public SpanPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a span policy in status open.</summary>
    public static SpanPolicy NewSpanPolicy(string code, string title)
    {
        return new SpanPolicy(code, title, 0.0, 0, 0, 0, "", "", 0.0, SpanPolicyStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanPolicyStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanPolicyStatus next)
    {
        switch (Status)
        {
            case SpanPolicyStatus.Open:
                switch (next)
                {
                    case SpanPolicyStatus.InReview:
                        return true;
                }
                return false;
            case SpanPolicyStatus.InReview:
                return next == SpanPolicyStatus.Reopened || next == SpanPolicyStatus.Archived;
            case SpanPolicyStatus.Resolved:
                return next == SpanPolicyStatus.Archived;
            case SpanPolicyStatus.Reopened:
                switch (next)
                {
                    case SpanPolicyStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"span policy"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Maximum + (double)LeaveDays, 2);
    }

    public SpanPolicy Copy()
    {
        return new SpanPolicy(
            Code,
            Title,
            Maximum,
            LeaveDays,
            UpdatedAt,
            Attempts,
            Source,
            Location,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
