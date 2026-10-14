namespace Acme.Workforce.Compensation.EquityGrant.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant quota is in its life cycle.</summary>
public enum EquityGrantQuotaStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a equity grant quota and its current status.</summary>
public sealed class EquityGrantQuota(string code, string name, string unit, int updatedAt, string grade, int attempts, string phone, string jobCode, int quantity, string timezone, string currency, EquityGrantQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Grade { get; set; } = grade;
    public int Attempts { get; set; } = attempts;
    public string Phone { get; set; } = phone;
    public string JobCode { get; set; } = jobCode;
    public int Quantity { get; set; } = quantity;
    public string Timezone { get; set; } = timezone;
    public string Currency { get; set; } = currency;
    public EquityGrantQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant quota in status requested.</summary>
    public static EquityGrantQuota NewEquityGrantQuota(string code, string name)
    {
        return new EquityGrantQuota(code, name, "", 0, "", 0, "", "", 0, "", "", EquityGrantQuotaStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantQuotaStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantQuotaStatus next)
    {
        switch (Status)
        {
            case EquityGrantQuotaStatus.Requested:
                switch (next)
                {
                    case EquityGrantQuotaStatus.Expired:
                        return true;
                }
                return false;
            case EquityGrantQuotaStatus.Granted:
                return next == EquityGrantQuotaStatus.Revoked || next == EquityGrantQuotaStatus.Expired;
            case EquityGrantQuotaStatus.Revoked:
                switch (next)
                {
                    case EquityGrantQuotaStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"equity grant quota"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)UpdatedAt + (double)Attempts, 2);
    }

    public EquityGrantQuota Copy()
    {
        return new EquityGrantQuota(
            Code,
            Name,
            Unit,
            UpdatedAt,
            Grade,
            Attempts,
            Phone,
            JobCode,
            Quantity,
            Timezone,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
