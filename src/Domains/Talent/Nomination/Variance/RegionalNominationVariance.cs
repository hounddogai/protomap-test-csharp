namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional nomination variance is in its life cycle.</summary>
public enum RegionalNominationVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a regional nomination variance and its current status.</summary>
public sealed class RegionalNominationVariance(string code, string name, int attempts, int priority, double rate, double hours, double ratio, double margin, string managerId, RegionalNominationVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public int Priority { get; set; } = priority;
    public double Rate { get; set; } = rate;
    public double Hours { get; set; } = hours;
    public double Ratio { get; set; } = ratio;
    public double Margin { get; set; } = margin;
    public string ManagerId { get; set; } = managerId;
    public RegionalNominationVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a regional nomination variance in status created.</summary>
    public static RegionalNominationVariance NewRegionalNominationVariance(string code, string name)
    {
        return new RegionalNominationVariance(
            code,
            name,
            0,
            0,
            0.0,
            0.0,
            0.0,
            0.0,
            "",
            RegionalNominationVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalNominationVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalNominationVarianceStatus next)
    {
        switch (Status)
        {
            case RegionalNominationVarianceStatus.Created:
                switch (next)
                {
                    case RegionalNominationVarianceStatus.Published:
                    case RegionalNominationVarianceStatus.Retired:
                        return true;
                }
                return false;
            case RegionalNominationVarianceStatus.Validated:
                return next == RegionalNominationVarianceStatus.Published;
            case RegionalNominationVarianceStatus.Published:
                switch (next)
                {
                    case RegionalNominationVarianceStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalNominationVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional nomination variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Priority - Margin, 2);
    }

    public RegionalNominationVariance Copy()
    {
        return new RegionalNominationVariance(
            Code,
            Name,
            Attempts,
            Priority,
            Rate,
            Hours,
            Ratio,
            Margin,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
