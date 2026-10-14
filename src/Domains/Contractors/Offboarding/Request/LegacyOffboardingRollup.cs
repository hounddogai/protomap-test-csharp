namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy offboarding rollup is in its life cycle.</summary>
public enum LegacyOffboardingRollupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A legacy offboarding rollup as the offboarding team keeps it.</summary>
public sealed class LegacyOffboardingRollup(string code, string title, string summary, string timezone, string region, double fte, LegacyOffboardingRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public string Timezone { get; set; } = timezone;
    public string Region { get; set; } = region;
    public double Fte { get; set; } = fte;
    public LegacyOffboardingRollupStatus Status { get; set; } = status;

    /// <summary>Starts a legacy offboarding rollup in status draft.</summary>
    public static LegacyOffboardingRollup NewLegacyOffboardingRollup(string code, string title)
    {
        return new LegacyOffboardingRollup(code, title, "", "", "", 0.0, LegacyOffboardingRollupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyOffboardingRollupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyOffboardingRollupStatus next)
    {
        switch (Status)
        {
            case LegacyOffboardingRollupStatus.Draft:
                switch (next)
                {
                    case LegacyOffboardingRollupStatus.Closed:
                        return true;
                }
                return false;
            case LegacyOffboardingRollupStatus.Pending:
                switch (next)
                {
                    case LegacyOffboardingRollupStatus.Active:
                    case LegacyOffboardingRollupStatus.Suspended:
                        return true;
                }
                return false;
            case LegacyOffboardingRollupStatus.Active:
                return next == LegacyOffboardingRollupStatus.Suspended;
            case LegacyOffboardingRollupStatus.Suspended:
                switch (next)
                {
                    case LegacyOffboardingRollupStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyOffboardingRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy offboarding rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public LegacyOffboardingRollup Copy()
    {
        return new LegacyOffboardingRollup(Code, Title, Summary, Timezone, Region, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
