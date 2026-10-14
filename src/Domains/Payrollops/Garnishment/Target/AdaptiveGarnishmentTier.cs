namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive garnishment tier is in its life cycle.</summary>
public enum AdaptiveGarnishmentTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A adaptive garnishment tier. Its code is unique within a tenant.</summary>
public sealed class AdaptiveGarnishmentTier(string code, string name, string timezone, double maximum, double weight, string checksum, string employeeId, int graceDays, int limit, bool approved, AdaptiveGarnishmentTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public double Maximum { get; set; } = maximum;
    public double Weight { get; set; } = weight;
    public string Checksum { get; set; } = checksum;
    public string EmployeeId { get; set; } = employeeId;
    public int GraceDays { get; set; } = graceDays;
    public int Limit { get; set; } = limit;
    public bool Approved { get; set; } = approved;
    public AdaptiveGarnishmentTierStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive garnishment tier in status draft.</summary>
    public static AdaptiveGarnishmentTier NewAdaptiveGarnishmentTier(string code, string name)
    {
        return new AdaptiveGarnishmentTier(
            code,
            name,
            "",
            0.0,
            0.0,
            "",
            "",
            0,
            0,
            false,
            AdaptiveGarnishmentTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveGarnishmentTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveGarnishmentTierStatus next)
    {
        switch (Status)
        {
            case AdaptiveGarnishmentTierStatus.Draft:
                switch (next)
                {
                    case AdaptiveGarnishmentTierStatus.Suspended:
                    case AdaptiveGarnishmentTierStatus.Closed:
                        return true;
                }
                return false;
            case AdaptiveGarnishmentTierStatus.Pending:
                return next == AdaptiveGarnishmentTierStatus.Suspended || next == AdaptiveGarnishmentTierStatus.Closed;
            case AdaptiveGarnishmentTierStatus.Active:
                switch (next)
                {
                    case AdaptiveGarnishmentTierStatus.Closed:
                        return true;
                }
                return false;
            case AdaptiveGarnishmentTierStatus.Suspended:
                return next == AdaptiveGarnishmentTierStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AdaptiveGarnishmentTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive garnishment tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"adaptive garnishment tier"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)GraceDays + (double)Limit, 2);
    }

    public AdaptiveGarnishmentTier Copy()
    {
        return new AdaptiveGarnishmentTier(
            Code,
            Name,
            Timezone,
            Maximum,
            Weight,
            Checksum,
            EmployeeId,
            GraceDays,
            Limit,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
