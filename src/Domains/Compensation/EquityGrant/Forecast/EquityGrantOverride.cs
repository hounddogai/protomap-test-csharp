namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant override is in its life cycle.</summary>
public enum EquityGrantOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A equity grant override as the equity grant team keeps it.</summary>
public sealed class EquityGrantOverride(string code, string name, double discount, double floorValue, string salaryBand, string costCenter, int expiresAt, double ratio, int endsAt, string timezone, EquityGrantOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public double FloorValue { get; set; } = floorValue;
    public string SalaryBand { get; set; } = salaryBand;
    public string CostCenter { get; set; } = costCenter;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Ratio { get; set; } = ratio;
    public int EndsAt { get; set; } = endsAt;
    public string Timezone { get; set; } = timezone;
    public EquityGrantOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant override in status open.</summary>
    public static EquityGrantOverride NewEquityGrantOverride(string code, string name)
    {
        return new EquityGrantOverride(code, name, 0.0, 0.0, "", "", 0, 0.0, 0, "", EquityGrantOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantOverrideStatus next)
    {
        switch (Status)
        {
            case EquityGrantOverrideStatus.Open:
                return next == EquityGrantOverrideStatus.InReview || next == EquityGrantOverrideStatus.Resolved;
            case EquityGrantOverrideStatus.InReview:
                return next == EquityGrantOverrideStatus.Resolved || next == EquityGrantOverrideStatus.Reopened;
            case EquityGrantOverrideStatus.Resolved:
                switch (next)
                {
                    case EquityGrantOverrideStatus.Reopened:
                    case EquityGrantOverrideStatus.Archived:
                        return true;
                }
                return false;
            case EquityGrantOverrideStatus.Reopened:
                switch (next)
                {
                    case EquityGrantOverrideStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"equity grant override"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)ExpiresAt - Ratio, 2);
    }

    public EquityGrantOverride Copy()
    {
        return new EquityGrantOverride(
            Code,
            Name,
            Discount,
            FloorValue,
            SalaryBand,
            CostCenter,
            ExpiresAt,
            Ratio,
            EndsAt,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
