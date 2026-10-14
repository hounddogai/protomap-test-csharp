namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling nomination threshold is in its life cycle.</summary>
public enum RollingNominationThresholdStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A rolling nomination threshold as the nomination team keeps it.</summary>
public sealed class RollingNominationThreshold(string code, string title, string category, string locale, string grade, string costCenter, string summary, double weight, double threshold, string country, RollingNominationThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string Locale { get; set; } = locale;
    public string Grade { get; set; } = grade;
    public string CostCenter { get; set; } = costCenter;
    public string Summary { get; set; } = summary;
    public double Weight { get; set; } = weight;
    public double Threshold { get; set; } = threshold;
    public string Country { get; set; } = country;
    public RollingNominationThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a rolling nomination threshold in status created.</summary>
    public static RollingNominationThreshold NewRollingNominationThreshold(string code, string title)
    {
        return new RollingNominationThreshold(
            code,
            title,
            "",
            "",
            "",
            "",
            "",
            0.0,
            0.0,
            "",
            RollingNominationThresholdStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingNominationThresholdStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingNominationThresholdStatus next)
    {
        switch (Status)
        {
            case RollingNominationThresholdStatus.Created:
                switch (next)
                {
                    case RollingNominationThresholdStatus.Published:
                    case RollingNominationThresholdStatus.Retired:
                        return true;
                }
                return false;
            case RollingNominationThresholdStatus.Validated:
                return next == RollingNominationThresholdStatus.Published;
            case RollingNominationThresholdStatus.Published:
                return next == RollingNominationThresholdStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RollingNominationThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling nomination threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Weight + Threshold, 2);
    }

    public RollingNominationThreshold Copy()
    {
        return new RollingNominationThreshold(
            Code,
            Title,
            Category,
            Locale,
            Grade,
            CostCenter,
            Summary,
            Weight,
            Threshold,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
