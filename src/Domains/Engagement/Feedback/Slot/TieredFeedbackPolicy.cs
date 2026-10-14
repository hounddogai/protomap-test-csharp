namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered feedback policy is in its life cycle.</summary>
public enum TieredFeedbackPolicyStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tiered feedback policy. Its code is unique within a tenant.</summary>
public sealed class TieredFeedbackPolicy(string code, string title, double rate, string description, string region, string timezone, double fte, bool archived, string contactEmail, string source, double cost, TieredFeedbackPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public string Description { get; set; } = description;
    public string Region { get; set; } = region;
    public string Timezone { get; set; } = timezone;
    public double Fte { get; set; } = fte;
    public bool Archived { get; set; } = archived;
    public string ContactEmail { get; set; } = contactEmail;
    public string Source { get; set; } = source;
    public double Cost { get; set; } = cost;
    public TieredFeedbackPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a tiered feedback policy in status requested.</summary>
    public static TieredFeedbackPolicy NewTieredFeedbackPolicy(string code, string title)
    {
        return new TieredFeedbackPolicy(
            code,
            title,
            0.0,
            "",
            "",
            "",
            0.0,
            false,
            "",
            "",
            0.0,
            TieredFeedbackPolicyStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredFeedbackPolicyStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredFeedbackPolicyStatus next)
    {
        switch (Status)
        {
            case TieredFeedbackPolicyStatus.Requested:
                return next == TieredFeedbackPolicyStatus.Revoked;
            case TieredFeedbackPolicyStatus.Granted:
                switch (next)
                {
                    case TieredFeedbackPolicyStatus.Expired:
                        return true;
                }
                return false;
            case TieredFeedbackPolicyStatus.Revoked:
                switch (next)
                {
                    case TieredFeedbackPolicyStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredFeedbackPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered feedback policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetValue()
    {
        return Math.Round(Rate + Cost, 2);
    }

    public TieredFeedbackPolicy Copy()
    {
        return new TieredFeedbackPolicy(
            Code,
            Title,
            Rate,
            Description,
            Region,
            Timezone,
            Fte,
            Archived,
            ContactEmail,
            Source,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
