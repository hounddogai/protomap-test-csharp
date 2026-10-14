namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional policy variance is in its life cycle.</summary>
public enum RegionalPolicyVarianceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A regional policy variance. Its code is unique within a tenant.</summary>
public sealed class RegionalPolicyVariance(string code, string name, double hours, int priority, int retries, string title, string skill, bool visible, int endsAt, double rate, double threshold, RegionalPolicyVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public int Priority { get; set; } = priority;
    public int Retries { get; set; } = retries;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public bool Visible { get; set; } = visible;
    public int EndsAt { get; set; } = endsAt;
    public double Rate { get; set; } = rate;
    public double Threshold { get; set; } = threshold;
    public RegionalPolicyVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a regional policy variance in status requested.</summary>
    public static RegionalPolicyVariance NewRegionalPolicyVariance(string code, string name)
    {
        return new RegionalPolicyVariance(
            code,
            name,
            0.0,
            0,
            0,
            "",
            "",
            false,
            0,
            0.0,
            0.0,
            RegionalPolicyVarianceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPolicyVarianceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPolicyVarianceStatus next)
    {
        switch (Status)
        {
            case RegionalPolicyVarianceStatus.Requested:
                return next == RegionalPolicyVarianceStatus.Revoked || next == RegionalPolicyVarianceStatus.Expired;
            case RegionalPolicyVarianceStatus.Granted:
                switch (next)
                {
                    case RegionalPolicyVarianceStatus.Revoked:
                        return true;
                }
                return false;
            case RegionalPolicyVarianceStatus.Revoked:
                return next == RegionalPolicyVarianceStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RegionalPolicyVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional policy variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)EndsAt - Hours, 2);
    }

    public RegionalPolicyVariance Copy()
    {
        return new RegionalPolicyVariance(
            Code,
            Name,
            Hours,
            Priority,
            Retries,
            Title,
            Skill,
            Visible,
            EndsAt,
            Rate,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
