namespace Acme.Workforce.Talent.Mobility.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility override is in its life cycle.</summary>
public enum MobilityOverrideStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a mobility override and its current status.</summary>
public sealed class MobilityOverride(string code, string name, int leaveDays, double hours, double weight, int capacity, string locale, double fte, string currency, string skill, string reviewer, MobilityOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public double Hours { get; set; } = hours;
    public double Weight { get; set; } = weight;
    public int Capacity { get; set; } = capacity;
    public string Locale { get; set; } = locale;
    public double Fte { get; set; } = fte;
    public string Currency { get; set; } = currency;
    public string Skill { get; set; } = skill;
    public string Reviewer { get; set; } = reviewer;
    public MobilityOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a mobility override in status created.</summary>
    public static MobilityOverride NewMobilityOverride(string code, string name)
    {
        return new MobilityOverride(code, name, 0, 0.0, 0.0, 0, "", 0.0, "", "", "", MobilityOverrideStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityOverrideStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityOverrideStatus next)
    {
        switch (Status)
        {
            case MobilityOverrideStatus.Created:
                switch (next)
                {
                    case MobilityOverrideStatus.Validated:
                    case MobilityOverrideStatus.Published:
                        return true;
                }
                return false;
            case MobilityOverrideStatus.Validated:
                switch (next)
                {
                    case MobilityOverrideStatus.Published:
                    case MobilityOverrideStatus.Retired:
                        return true;
                }
                return false;
            case MobilityOverrideStatus.Published:
                return next == MobilityOverrideStatus.Retired;
        }
        return false;
    }

    public void MoveTo(MobilityOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Fte * (double)LeaveDays, 2);
    }

    public MobilityOverride Copy()
    {
        return new MobilityOverride(
            Code,
            Name,
            LeaveDays,
            Hours,
            Weight,
            Capacity,
            Locale,
            Fte,
            Currency,
            Skill,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
