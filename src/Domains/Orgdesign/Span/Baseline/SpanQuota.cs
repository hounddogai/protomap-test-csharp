namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span quota is in its life cycle.</summary>
public enum SpanQuotaStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A span quota as the span team keeps it.</summary>
public sealed class SpanQuota(string code, string label, string badge, string name, string contactEmail, int tenureMonths, string currency, SpanQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Currency { get; set; } = currency;
    public SpanQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a span quota in status requested.</summary>
    public static SpanQuota NewSpanQuota(string code, string label)
    {
        return new SpanQuota(code, label, "", "", "", 0, "", SpanQuotaStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanQuotaStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanQuotaStatus next)
    {
        switch (Status)
        {
            case SpanQuotaStatus.Requested:
                switch (next)
                {
                    case SpanQuotaStatus.Revoked:
                    case SpanQuotaStatus.Expired:
                        return true;
                }
                return false;
            case SpanQuotaStatus.Granted:
                switch (next)
                {
                    case SpanQuotaStatus.Revoked:
                    case SpanQuotaStatus.Expired:
                        return true;
                }
                return false;
            case SpanQuotaStatus.Revoked:
                return next == SpanQuotaStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SpanQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"span quota"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public SpanQuota Copy()
    {
        return new SpanQuota(Code, Label, Badge, Name, ContactEmail, TenureMonths, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
