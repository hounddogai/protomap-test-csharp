namespace Acme.Workforce.Payrollops.Deduction.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction quota is in its life cycle.</summary>
public enum DeductionQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a deduction quota and its current status.</summary>
public sealed class DeductionQuota(string code, string name, string country, int leaveDays, int updatedAt, string source, bool approved, string phone, double cost, DeductionQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public int LeaveDays { get; set; } = leaveDays;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Source { get; set; } = source;
    public bool Approved { get; set; } = approved;
    public string Phone { get; set; } = phone;
    public double Cost { get; set; } = cost;
    public DeductionQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a deduction quota in status created.</summary>
    public static DeductionQuota NewDeductionQuota(string code, string name)
    {
        return new DeductionQuota(code, name, "", 0, 0, "", false, "", 0.0, DeductionQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionQuotaStatus next)
    {
        switch (Status)
        {
            case DeductionQuotaStatus.Created:
                switch (next)
                {
                    case DeductionQuotaStatus.Validated:
                    case DeductionQuotaStatus.Published:
                        return true;
                }
                return false;
            case DeductionQuotaStatus.Validated:
                return next == DeductionQuotaStatus.Published;
            case DeductionQuotaStatus.Published:
                return next == DeductionQuotaStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DeductionQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"deduction quota"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Cost + (double)UpdatedAt, 2);
    }

    public DeductionQuota Copy()
    {
        return new DeductionQuota(Code, Name, Country, LeaveDays, UpdatedAt, Source, Approved, Phone, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
