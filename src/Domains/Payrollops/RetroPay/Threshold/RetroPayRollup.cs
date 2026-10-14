namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay rollup is in its life cycle.</summary>
public enum RetroPayRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a retro pay rollup and its current status.</summary>
public sealed class RetroPayRollup(string code, string title, string costCenter, string source, int expiresAt, string label, string locale, string country, string ownerId, string badge, RetroPayRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public string Source { get; set; } = source;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public string Country { get; set; } = country;
    public string OwnerId { get; set; } = ownerId;
    public string Badge { get; set; } = badge;
    public RetroPayRollupStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay rollup in status created.</summary>
    public static RetroPayRollup NewRetroPayRollup(string code, string title)
    {
        return new RetroPayRollup(code, title, "", "", 0, "", "", "", "", "", RetroPayRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayRollupStatus next)
    {
        switch (Status)
        {
            case RetroPayRollupStatus.Created:
                return next == RetroPayRollupStatus.Validated || next == RetroPayRollupStatus.Published;
            case RetroPayRollupStatus.Validated:
                switch (next)
                {
                    case RetroPayRollupStatus.Published:
                    case RetroPayRollupStatus.Retired:
                        return true;
                }
                return false;
            case RetroPayRollupStatus.Published:
                return next == RetroPayRollupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RetroPayRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public RetroPayRollup Copy()
    {
        return new RetroPayRollup(
            Code,
            Title,
            CostCenter,
            Source,
            ExpiresAt,
            Label,
            Locale,
            Country,
            OwnerId,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
