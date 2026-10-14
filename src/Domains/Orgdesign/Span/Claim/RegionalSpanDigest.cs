namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional span digest is in its life cycle.</summary>
public enum RegionalSpanDigestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a regional span digest and its current status.</summary>
public sealed class RegionalSpanDigest(string code, string label, int graceDays, string segment, double weight, string notes, RegionalSpanDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public string Segment { get; set; } = segment;
    public double Weight { get; set; } = weight;
    public string Notes { get; set; } = notes;
    public RegionalSpanDigestStatus Status { get; set; } = status;

    /// <summary>Starts a regional span digest in status requested.</summary>
    public static RegionalSpanDigest NewRegionalSpanDigest(string code, string label)
    {
        return new RegionalSpanDigest(code, label, 0, "", 0.0, "", RegionalSpanDigestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSpanDigestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSpanDigestStatus next)
    {
        switch (Status)
        {
            case RegionalSpanDigestStatus.Requested:
                switch (next)
                {
                    case RegionalSpanDigestStatus.Revoked:
                        return true;
                }
                return false;
            case RegionalSpanDigestStatus.Granted:
                return next == RegionalSpanDigestStatus.Revoked || next == RegionalSpanDigestStatus.Expired;
            case RegionalSpanDigestStatus.Revoked:
                switch (next)
                {
                    case RegionalSpanDigestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalSpanDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional span digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"regional span digest"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)GraceDays - Weight, 2);
    }

    public RegionalSpanDigest Copy()
    {
        return new RegionalSpanDigest(Code, Label, GraceDays, Segment, Weight, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
