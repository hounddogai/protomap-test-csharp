namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional engagement notice is in its life cycle.</summary>
public enum RegionalEngagementNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A regional engagement notice. Its code is unique within a tenant.</summary>
public sealed class RegionalEngagementNotice(string code, string title, int quantity, double discount, int expiresAt, RegionalEngagementNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public double Discount { get; set; } = discount;
    public int ExpiresAt { get; set; } = expiresAt;
    public RegionalEngagementNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a regional engagement notice in status created.</summary>
    public static RegionalEngagementNotice NewRegionalEngagementNotice(string code, string title)
    {
        return new RegionalEngagementNotice(code, title, 0, 0.0, 0, RegionalEngagementNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalEngagementNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalEngagementNoticeStatus next)
    {
        switch (Status)
        {
            case RegionalEngagementNoticeStatus.Created:
                switch (next)
                {
                    case RegionalEngagementNoticeStatus.Validated:
                    case RegionalEngagementNoticeStatus.Retired:
                        return true;
                }
                return false;
            case RegionalEngagementNoticeStatus.Validated:
                switch (next)
                {
                    case RegionalEngagementNoticeStatus.Published:
                    case RegionalEngagementNoticeStatus.Retired:
                        return true;
                }
                return false;
            case RegionalEngagementNoticeStatus.Published:
                return next == RegionalEngagementNoticeStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RegionalEngagementNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional engagement notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Quantity * (double)ExpiresAt, 2);
    }

    public RegionalEngagementNotice Copy()
    {
        return new RegionalEngagementNotice(Code, Title, Quantity, Discount, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
