namespace Acme.Workforce.Workplace.AccessBadge.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge segment is in its life cycle.</summary>
public enum AccessBadgeSegmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a access badge segment and its current status.</summary>
public sealed class AccessBadgeSegment(string code, string name, string source, string checksum, int expiresAt, AccessBadgeSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public string Checksum { get; set; } = checksum;
    public int ExpiresAt { get; set; } = expiresAt;
    public AccessBadgeSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a access badge segment in status created.</summary>
    public static AccessBadgeSegment NewAccessBadgeSegment(string code, string name)
    {
        return new AccessBadgeSegment(code, name, "", "", 0, AccessBadgeSegmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeSegmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeSegmentStatus next)
    {
        switch (Status)
        {
            case AccessBadgeSegmentStatus.Created:
                switch (next)
                {
                    case AccessBadgeSegmentStatus.Published:
                    case AccessBadgeSegmentStatus.Retired:
                        return true;
                }
                return false;
            case AccessBadgeSegmentStatus.Validated:
                return next == AccessBadgeSegmentStatus.Retired;
            case AccessBadgeSegmentStatus.Published:
                switch (next)
                {
                    case AccessBadgeSegmentStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public AccessBadgeSegment Copy()
    {
        return new AccessBadgeSegment(Code, Name, Source, Checksum, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
