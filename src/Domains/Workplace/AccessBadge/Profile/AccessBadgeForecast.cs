namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge forecast is in its life cycle.</summary>
public enum AccessBadgeForecastStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A access badge forecast. Its code is unique within a tenant.</summary>
public sealed class AccessBadgeForecast(string code, string name, bool pinned, int capacity, string timezone, string managerId, string unit, double threshold, int retries, double ratio, AccessBadgeForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public int Capacity { get; set; } = capacity;
    public string Timezone { get; set; } = timezone;
    public string ManagerId { get; set; } = managerId;
    public string Unit { get; set; } = unit;
    public double Threshold { get; set; } = threshold;
    public int Retries { get; set; } = retries;
    public double Ratio { get; set; } = ratio;
    public AccessBadgeForecastStatus Status { get; set; } = status;

    /// <summary>Starts a access badge forecast in status requested.</summary>
    public static AccessBadgeForecast NewAccessBadgeForecast(string code, string name)
    {
        return new AccessBadgeForecast(
            code,
            name,
            false,
            0,
            "",
            "",
            "",
            0.0,
            0,
            0.0,
            AccessBadgeForecastStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeForecastStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeForecastStatus next)
    {
        switch (Status)
        {
            case AccessBadgeForecastStatus.Requested:
                return next == AccessBadgeForecastStatus.Granted;
            case AccessBadgeForecastStatus.Granted:
                return next == AccessBadgeForecastStatus.Revoked || next == AccessBadgeForecastStatus.Expired;
            case AccessBadgeForecastStatus.Revoked:
                return next == AccessBadgeForecastStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AccessBadgeForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Capacity * Ratio, 2);
    }

    public AccessBadgeForecast Copy()
    {
        return new AccessBadgeForecast(
            Code,
            Name,
            Pinned,
            Capacity,
            Timezone,
            ManagerId,
            Unit,
            Threshold,
            Retries,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
