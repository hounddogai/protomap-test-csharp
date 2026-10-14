namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band quota is in its life cycle.</summary>
public enum PayBandQuotaStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One pay band quota, with what the planners need to act on it.</summary>
public sealed class PayBandQuota(string code, string name, double amount, Dictionary<string, string> attributes, double hours, int expiresAt, string region, PayBandQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Hours { get; set; } = hours;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Region { get; set; } = region;
    public PayBandQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a pay band quota in status new.</summary>
    public static PayBandQuota NewPayBandQuota(string code, string name)
    {
        return new PayBandQuota(
            code,
            name,
            0.0,
            new Dictionary<string, string>(),
            0.0,
            0,
            "",
            PayBandQuotaStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandQuotaStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandQuotaStatus next)
    {
        switch (Status)
        {
            case PayBandQuotaStatus.New:
                switch (next)
                {
                    case PayBandQuotaStatus.Queued:
                    case PayBandQuotaStatus.Failed:
                        return true;
                }
                return false;
            case PayBandQuotaStatus.Queued:
                switch (next)
                {
                    case PayBandQuotaStatus.Done:
                    case PayBandQuotaStatus.Failed:
                        return true;
                }
                return false;
            case PayBandQuotaStatus.Running:
                switch (next)
                {
                    case PayBandQuotaStatus.Done:
                    case PayBandQuotaStatus.Failed:
                        return true;
                }
                return false;
            case PayBandQuotaStatus.Done:
                switch (next)
                {
                    case PayBandQuotaStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double EffectiveTotal()
    {
        return Math.Round(Hours - (double)ExpiresAt, 2);
    }

    public PayBandQuota Copy()
    {
        return new PayBandQuota(Code, Name, Amount, Attributes, Hours, ExpiresAt, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
