namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default adjustment agreement is in its life cycle.</summary>
public enum DefaultAdjustmentAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A default adjustment agreement. Its code is unique within a tenant.</summary>
public sealed class DefaultAdjustmentAgreement(string code, string title, string team, int capacity, string costCenter, string timezone, int priority, string country, double minimum, DefaultAdjustmentAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public int Capacity { get; set; } = capacity;
    public string CostCenter { get; set; } = costCenter;
    public string Timezone { get; set; } = timezone;
    public int Priority { get; set; } = priority;
    public string Country { get; set; } = country;
    public double Minimum { get; set; } = minimum;
    public DefaultAdjustmentAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a default adjustment agreement in status created.</summary>
    public static DefaultAdjustmentAgreement NewDefaultAdjustmentAgreement(string code, string title)
    {
        return new DefaultAdjustmentAgreement(
            code,
            title,
            "",
            0,
            "",
            "",
            0,
            "",
            0.0,
            DefaultAdjustmentAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAdjustmentAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAdjustmentAgreementStatus next)
    {
        switch (Status)
        {
            case DefaultAdjustmentAgreementStatus.Created:
                return next == DefaultAdjustmentAgreementStatus.Validated || next == DefaultAdjustmentAgreementStatus.Published;
            case DefaultAdjustmentAgreementStatus.Validated:
                switch (next)
                {
                    case DefaultAdjustmentAgreementStatus.Published:
                        return true;
                }
                return false;
            case DefaultAdjustmentAgreementStatus.Published:
                return next == DefaultAdjustmentAgreementStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DefaultAdjustmentAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default adjustment agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Priority - (double)Capacity, 2);
    }

    public DefaultAdjustmentAgreement Copy()
    {
        return new DefaultAdjustmentAgreement(
            Code,
            Title,
            Team,
            Capacity,
            CostCenter,
            Timezone,
            Priority,
            Country,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
