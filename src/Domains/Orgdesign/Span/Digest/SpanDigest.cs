namespace Acme.Workforce.Orgdesign.Span.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span digest is in its life cycle.</summary>
public enum SpanDigestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A span digest. Its code is unique within a tenant.</summary>
public sealed class SpanDigest(string code, string name, double margin, double threshold, string title, double weight, double fte, SpanDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public double Threshold { get; set; } = threshold;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public double Fte { get; set; } = fte;
    public SpanDigestStatus Status { get; set; } = status;

    /// <summary>Starts a span digest in status requested.</summary>
    public static SpanDigest NewSpanDigest(string code, string name)
    {
        return new SpanDigest(code, name, 0.0, 0.0, "", 0.0, 0.0, SpanDigestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanDigestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanDigestStatus next)
    {
        switch (Status)
        {
            case SpanDigestStatus.Requested:
                switch (next)
                {
                    case SpanDigestStatus.Granted:
                        return true;
                }
                return false;
            case SpanDigestStatus.Granted:
                switch (next)
                {
                    case SpanDigestStatus.Revoked:
                        return true;
                }
                return false;
            case SpanDigestStatus.Revoked:
                return next == SpanDigestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SpanDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"span digest"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Margin + Fte, 2);
    }

    public SpanDigest Copy()
    {
        return new SpanDigest(Code, Name, Margin, Threshold, Title, Weight, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
