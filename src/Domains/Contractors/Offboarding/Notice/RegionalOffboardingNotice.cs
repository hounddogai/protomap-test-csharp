namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional offboarding notice is in its life cycle.</summary>
public enum RegionalOffboardingNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a regional offboarding notice and its current status.</summary>
public sealed class RegionalOffboardingNotice(string code, string label, double cost, string unit, string description, string country, RegionalOffboardingNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public string Unit { get; set; } = unit;
    public string Description { get; set; } = description;
    public string Country { get; set; } = country;
    public RegionalOffboardingNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a regional offboarding notice in status draft.</summary>
    public static RegionalOffboardingNotice NewRegionalOffboardingNotice(string code, string label)
    {
        return new RegionalOffboardingNotice(code, label, 0.0, "", "", "", RegionalOffboardingNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalOffboardingNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalOffboardingNoticeStatus next)
    {
        switch (Status)
        {
            case RegionalOffboardingNoticeStatus.Draft:
                return next == RegionalOffboardingNoticeStatus.Pending;
            case RegionalOffboardingNoticeStatus.Pending:
                switch (next)
                {
                    case RegionalOffboardingNoticeStatus.Suspended:
                        return true;
                }
                return false;
            case RegionalOffboardingNoticeStatus.Active:
                switch (next)
                {
                    case RegionalOffboardingNoticeStatus.Suspended:
                        return true;
                }
                return false;
            case RegionalOffboardingNoticeStatus.Suspended:
                return next == RegionalOffboardingNoticeStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RegionalOffboardingNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional offboarding notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"regional offboarding notice"} {Code}";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public RegionalOffboardingNotice Copy()
    {
        return new RegionalOffboardingNotice(Code, Label, Cost, Unit, Description, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
