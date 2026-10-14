namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment tier is in its life cycle.</summary>
public enum AdjustmentTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One adjustment tier, with what the planners need to act on it.</summary>
public sealed class AdjustmentTier(string code, string title, bool active, string jobCode, int updatedAt, string locale, string employeeId, int windowDays, string grade, int tenureMonths, string unit, AdjustmentTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public string JobCode { get; set; } = jobCode;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Locale { get; set; } = locale;
    public string EmployeeId { get; set; } = employeeId;
    public int WindowDays { get; set; } = windowDays;
    public string Grade { get; set; } = grade;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Unit { get; set; } = unit;
    public AdjustmentTierStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment tier in status created.</summary>
    public static AdjustmentTier NewAdjustmentTier(string code, string title)
    {
        return new AdjustmentTier(code, title, false, "", 0, "", "", 0, "", 0, "", AdjustmentTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentTierStatus next)
    {
        switch (Status)
        {
            case AdjustmentTierStatus.Created:
                switch (next)
                {
                    case AdjustmentTierStatus.Validated:
                    case AdjustmentTierStatus.Published:
                        return true;
                }
                return false;
            case AdjustmentTierStatus.Validated:
                return next == AdjustmentTierStatus.Published;
            case AdjustmentTierStatus.Published:
                return next == AdjustmentTierStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdjustmentTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)TenureMonths * (double)UpdatedAt, 2);
    }

    public AdjustmentTier Copy()
    {
        return new AdjustmentTier(
            Code,
            Title,
            Active,
            JobCode,
            UpdatedAt,
            Locale,
            EmployeeId,
            WindowDays,
            Grade,
            TenureMonths,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
