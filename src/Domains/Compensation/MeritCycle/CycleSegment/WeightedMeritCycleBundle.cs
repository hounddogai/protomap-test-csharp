namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted merit cycle bundle is in its life cycle.</summary>
public enum WeightedMeritCycleBundleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a weighted merit cycle bundle and its current status.</summary>
public sealed class WeightedMeritCycleBundle(string code, string label, int sequence, int tenureMonths, string country, int retries, double budget, bool pinned, double margin, double floorValue, WeightedMeritCycleBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Country { get; set; } = country;
    public int Retries { get; set; } = retries;
    public double Budget { get; set; } = budget;
    public bool Pinned { get; set; } = pinned;
    public double Margin { get; set; } = margin;
    public double FloorValue { get; set; } = floorValue;
    public WeightedMeritCycleBundleStatus Status { get; set; } = status;

    /// <summary>Starts a weighted merit cycle bundle in status open.</summary>
    public static WeightedMeritCycleBundle NewWeightedMeritCycleBundle(string code, string label)
    {
        return new WeightedMeritCycleBundle(
            code,
            label,
            0,
            0,
            "",
            0,
            0.0,
            false,
            0.0,
            0.0,
            WeightedMeritCycleBundleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedMeritCycleBundleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedMeritCycleBundleStatus next)
    {
        switch (Status)
        {
            case WeightedMeritCycleBundleStatus.Open:
                return next == WeightedMeritCycleBundleStatus.InReview;
            case WeightedMeritCycleBundleStatus.InReview:
                return next == WeightedMeritCycleBundleStatus.Resolved || next == WeightedMeritCycleBundleStatus.Archived;
            case WeightedMeritCycleBundleStatus.Resolved:
                switch (next)
                {
                    case WeightedMeritCycleBundleStatus.Reopened:
                    case WeightedMeritCycleBundleStatus.Archived:
                        return true;
                }
                return false;
            case WeightedMeritCycleBundleStatus.Reopened:
                return next == WeightedMeritCycleBundleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(WeightedMeritCycleBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted merit cycle bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(FloorValue - (double)Retries, 2);
    }

    public WeightedMeritCycleBundle Copy()
    {
        return new WeightedMeritCycleBundle(
            Code,
            Label,
            Sequence,
            TenureMonths,
            Country,
            Retries,
            Budget,
            Pinned,
            Margin,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
