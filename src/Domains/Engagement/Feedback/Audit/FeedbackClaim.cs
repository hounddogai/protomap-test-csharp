namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback claim is in its life cycle.</summary>
public enum FeedbackClaimStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One feedback claim, with what the planners need to act on it.</summary>
public sealed class FeedbackClaim(string code, string name, int leaveDays, double ratio, string region, Dictionary<string, string> attributes, string timezone, FeedbackClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public double Ratio { get; set; } = ratio;
    public string Region { get; set; } = region;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Timezone { get; set; } = timezone;
    public FeedbackClaimStatus Status { get; set; } = status;

    /// <summary>Starts a feedback claim in status requested.</summary>
    public static FeedbackClaim NewFeedbackClaim(string code, string name)
    {
        return new FeedbackClaim(
            code,
            name,
            0,
            0.0,
            "",
            new Dictionary<string, string>(),
            "",
            FeedbackClaimStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackClaimStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackClaimStatus next)
    {
        switch (Status)
        {
            case FeedbackClaimStatus.Requested:
                switch (next)
                {
                    case FeedbackClaimStatus.Granted:
                    case FeedbackClaimStatus.Revoked:
                        return true;
                }
                return false;
            case FeedbackClaimStatus.Granted:
                switch (next)
                {
                    case FeedbackClaimStatus.Revoked:
                        return true;
                }
                return false;
            case FeedbackClaimStatus.Revoked:
                return next == FeedbackClaimStatus.Expired;
        }
        return false;
    }

    public void MoveTo(FeedbackClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double NetLoad()
    {
        return Math.Round(Ratio - (double)LeaveDays, 2);
    }

    public FeedbackClaim Copy()
    {
        return new FeedbackClaim(Code, Name, LeaveDays, Ratio, Region, Attributes, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
