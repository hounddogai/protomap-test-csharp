namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pay band request is in its life cycle.</summary>
public enum LegacyPayBandRequestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a legacy pay band request and its current status.</summary>
public sealed class LegacyPayBandRequest(string code, string name, string team, bool active, Dictionary<string, string> attributes, int expiresAt, LegacyPayBandRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public bool Active { get; set; } = active;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int ExpiresAt { get; set; } = expiresAt;
    public LegacyPayBandRequestStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pay band request in status open.</summary>
    public static LegacyPayBandRequest NewLegacyPayBandRequest(string code, string name)
    {
        return new LegacyPayBandRequest(
            code,
            name,
            "",
            false,
            new Dictionary<string, string>(),
            0,
            LegacyPayBandRequestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPayBandRequestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPayBandRequestStatus next)
    {
        switch (Status)
        {
            case LegacyPayBandRequestStatus.Open:
                return next == LegacyPayBandRequestStatus.Archived;
            case LegacyPayBandRequestStatus.InReview:
                switch (next)
                {
                    case LegacyPayBandRequestStatus.Resolved:
                        return true;
                }
                return false;
            case LegacyPayBandRequestStatus.Resolved:
                return next == LegacyPayBandRequestStatus.Archived;
            case LegacyPayBandRequestStatus.Reopened:
                return next == LegacyPayBandRequestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LegacyPayBandRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pay band request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"legacy pay band request"} {Code}";
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

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public LegacyPayBandRequest Copy()
    {
        return new LegacyPayBandRequest(Code, Name, Team, Active, Attributes, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
