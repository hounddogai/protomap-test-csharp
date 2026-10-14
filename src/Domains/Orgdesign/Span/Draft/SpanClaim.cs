namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span claim is in its life cycle.</summary>
public enum SpanClaimStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A span claim as the span team keeps it.</summary>
public sealed class SpanClaim(string code, string name, double weight, string location, string country, SpanClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public string Location { get; set; } = location;
    public string Country { get; set; } = country;
    public SpanClaimStatus Status { get; set; } = status;

    /// <summary>Starts a span claim in status open.</summary>
    public static SpanClaim NewSpanClaim(string code, string name)
    {
        return new SpanClaim(code, name, 0.0, "", "", SpanClaimStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanClaimStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanClaimStatus next)
    {
        switch (Status)
        {
            case SpanClaimStatus.Open:
                return next == SpanClaimStatus.InReview || next == SpanClaimStatus.Resolved;
            case SpanClaimStatus.InReview:
                switch (next)
                {
                    case SpanClaimStatus.Resolved:
                    case SpanClaimStatus.Reopened:
                        return true;
                }
                return false;
            case SpanClaimStatus.Resolved:
                return next == SpanClaimStatus.Reopened;
            case SpanClaimStatus.Reopened:
                switch (next)
                {
                    case SpanClaimStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"span claim"} {Code}";
    }

    public double MeasuredValue()
    {
        return Weight;
    }

    public SpanClaim Copy()
    {
        return new SpanClaim(Code, Name, Weight, Location, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
