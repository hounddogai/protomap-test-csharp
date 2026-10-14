namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default warranty cycle is in its life cycle.</summary>
public enum DefaultWarrantyCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a default warranty cycle and its current status.</summary>
public sealed class DefaultWarrantyCycle(string code, string name, string summary, string timezone, string costCenter, string country, string locale, string segment, int limit, double rate, DefaultWarrantyCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public string Timezone { get; set; } = timezone;
    public string CostCenter { get; set; } = costCenter;
    public string Country { get; set; } = country;
    public string Locale { get; set; } = locale;
    public string Segment { get; set; } = segment;
    public int Limit { get; set; } = limit;
    public double Rate { get; set; } = rate;
    public DefaultWarrantyCycleStatus Status { get; set; } = status;

    /// <summary>Starts a default warranty cycle in status planned.</summary>
    public static DefaultWarrantyCycle NewDefaultWarrantyCycle(string code, string name)
    {
        return new DefaultWarrantyCycle(
            code,
            name,
            "",
            "",
            "",
            "",
            "",
            "",
            0,
            0.0,
            DefaultWarrantyCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultWarrantyCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultWarrantyCycleStatus next)
    {
        switch (Status)
        {
            case DefaultWarrantyCycleStatus.Planned:
                return next == DefaultWarrantyCycleStatus.Cancelled;
            case DefaultWarrantyCycleStatus.Scheduled:
                return next == DefaultWarrantyCycleStatus.Completed || next == DefaultWarrantyCycleStatus.Cancelled;
            case DefaultWarrantyCycleStatus.InProgress:
                return next == DefaultWarrantyCycleStatus.Completed;
            case DefaultWarrantyCycleStatus.Completed:
                return next == DefaultWarrantyCycleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(DefaultWarrantyCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default warranty cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default warranty cycle"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Limit - Rate, 2);
    }

    public DefaultWarrantyCycle Copy()
    {
        return new DefaultWarrantyCycle(
            Code,
            Name,
            Summary,
            Timezone,
            CostCenter,
            Country,
            Locale,
            Segment,
            Limit,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
