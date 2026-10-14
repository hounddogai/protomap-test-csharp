namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate notice is in its life cycle.</summary>
public enum RateNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One rate notice, with what the planners need to act on it.</summary>
public sealed class RateNotice(string code, string label, int expiresAt, int windowDays, string jobCode, double budget, List<string> tags, bool pinned, string phone, string badge, RateNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public int WindowDays { get; set; } = windowDays;
    public string JobCode { get; set; } = jobCode;
    public double Budget { get; set; } = budget;
    public List<string> Tags { get; set; } = tags;
    public bool Pinned { get; set; } = pinned;
    public string Phone { get; set; } = phone;
    public string Badge { get; set; } = badge;
    public RateNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a rate notice in status created.</summary>
    public static RateNotice NewRateNotice(string code, string label)
    {
        return new RateNotice(
            code,
            label,
            0,
            0,
            "",
            0.0,
            new List<string>(),
            false,
            "",
            "",
            RateNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateNoticeStatus next)
    {
        switch (Status)
        {
            case RateNoticeStatus.Created:
                switch (next)
                {
                    case RateNoticeStatus.Validated:
                    case RateNoticeStatus.Published:
                        return true;
                }
                return false;
            case RateNoticeStatus.Validated:
                return next == RateNoticeStatus.Retired;
            case RateNoticeStatus.Published:
                return next == RateNoticeStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RateNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"rate notice"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetVolume()
    {
        return Math.Round(Budget * (double)ExpiresAt, 2);
    }

    public RateNotice Copy()
    {
        return new RateNotice(
            Code,
            Label,
            ExpiresAt,
            WindowDays,
            JobCode,
            Budget,
            Tags,
            Pinned,
            Phone,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
