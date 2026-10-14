namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding bundle is in its life cycle.</summary>
public enum OffboardingBundleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One offboarding bundle, with what the planners need to act on it.</summary>
public sealed class OffboardingBundle(string code, string name, double rate, string timezone, double hours, string salaryBand, string channel, double budget, string reviewer, int revision, OffboardingBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string Timezone { get; set; } = timezone;
    public double Hours { get; set; } = hours;
    public string SalaryBand { get; set; } = salaryBand;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public string Reviewer { get; set; } = reviewer;
    public int Revision { get; set; } = revision;
    public OffboardingBundleStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding bundle in status requested.</summary>
    public static OffboardingBundle NewOffboardingBundle(string code, string name)
    {
        return new OffboardingBundle(
            code,
            name,
            0.0,
            "",
            0.0,
            "",
            "",
            0.0,
            "",
            0,
            OffboardingBundleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingBundleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingBundleStatus next)
    {
        switch (Status)
        {
            case OffboardingBundleStatus.Requested:
                return next == OffboardingBundleStatus.Granted;
            case OffboardingBundleStatus.Granted:
                return next == OffboardingBundleStatus.Revoked;
            case OffboardingBundleStatus.Revoked:
                switch (next)
                {
                    case OffboardingBundleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Rate - (double)Revision, 2);
    }

    public OffboardingBundle Copy()
    {
        return new OffboardingBundle(
            Code,
            Name,
            Rate,
            Timezone,
            Hours,
            SalaryBand,
            Channel,
            Budget,
            Reviewer,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
