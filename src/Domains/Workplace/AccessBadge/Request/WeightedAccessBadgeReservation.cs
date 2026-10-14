namespace Acme.Workforce.Workplace.AccessBadge.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted access badge reservation is in its life cycle.</summary>
public enum WeightedAccessBadgeReservationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A weighted access badge reservation as the access badge team keeps it.</summary>
public sealed class WeightedAccessBadgeReservation(string code, string title, double ratio, double score, string externalRef, int version, string costCenter, bool visible, string grade, string department, double ceiling, WeightedAccessBadgeReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public double Score { get; set; } = score;
    public string ExternalRef { get; set; } = externalRef;
    public int Version { get; set; } = version;
    public string CostCenter { get; set; } = costCenter;
    public bool Visible { get; set; } = visible;
    public string Grade { get; set; } = grade;
    public string Department { get; set; } = department;
    public double Ceiling { get; set; } = ceiling;
    public WeightedAccessBadgeReservationStatus Status { get; set; } = status;

    /// <summary>Starts a weighted access badge reservation in status created.</summary>
    public static WeightedAccessBadgeReservation NewWeightedAccessBadgeReservation(string code, string title)
    {
        return new WeightedAccessBadgeReservation(
            code,
            title,
            0.0,
            0.0,
            "",
            0,
            "",
            false,
            "",
            "",
            0.0,
            WeightedAccessBadgeReservationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAccessBadgeReservationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAccessBadgeReservationStatus next)
    {
        switch (Status)
        {
            case WeightedAccessBadgeReservationStatus.Created:
                switch (next)
                {
                    case WeightedAccessBadgeReservationStatus.Published:
                        return true;
                }
                return false;
            case WeightedAccessBadgeReservationStatus.Validated:
                return next == WeightedAccessBadgeReservationStatus.Published || next == WeightedAccessBadgeReservationStatus.Retired;
            case WeightedAccessBadgeReservationStatus.Published:
                switch (next)
                {
                    case WeightedAccessBadgeReservationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedAccessBadgeReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted access badge reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Ratio * Score, 2);
    }

    public WeightedAccessBadgeReservation Copy()
    {
        return new WeightedAccessBadgeReservation(
            Code,
            Title,
            Ratio,
            Score,
            ExternalRef,
            Version,
            CostCenter,
            Visible,
            Grade,
            Department,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
