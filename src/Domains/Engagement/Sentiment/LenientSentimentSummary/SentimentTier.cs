namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment tier is in its life cycle.</summary>
public enum SentimentTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A sentiment tier. Its code is unique within a tenant.</summary>
public sealed class SentimentTier(string code, string name, string team, double fte, double ceiling, string ownerId, string grade, string category, int createdAt, SentimentTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public double Fte { get; set; } = fte;
    public double Ceiling { get; set; } = ceiling;
    public string OwnerId { get; set; } = ownerId;
    public string Grade { get; set; } = grade;
    public string Category { get; set; } = category;
    public int CreatedAt { get; set; } = createdAt;
    public SentimentTierStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment tier in status created.</summary>
    public static SentimentTier NewSentimentTier(string code, string name)
    {
        return new SentimentTier(code, name, "", 0.0, 0.0, "", "", "", 0, SentimentTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentTierStatus next)
    {
        switch (Status)
        {
            case SentimentTierStatus.Created:
                switch (next)
                {
                    case SentimentTierStatus.Validated:
                        return true;
                }
                return false;
            case SentimentTierStatus.Validated:
                return next == SentimentTierStatus.Published || next == SentimentTierStatus.Retired;
            case SentimentTierStatus.Published:
                return next == SentimentTierStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SentimentTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)CreatedAt + Fte, 2);
    }

    public SentimentTier Copy()
    {
        return new SentimentTier(Code, Name, Team, Fte, Ceiling, OwnerId, Grade, Category, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
