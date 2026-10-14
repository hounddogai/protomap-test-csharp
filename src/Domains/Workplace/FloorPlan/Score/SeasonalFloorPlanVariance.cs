namespace Acme.Workforce.Workplace.FloorPlan.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal floor plan variance is in its life cycle.</summary>
public enum SeasonalFloorPlanVarianceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a seasonal floor plan variance and its current status.</summary>
public sealed class SeasonalFloorPlanVariance(string code, string label, string salaryBand, string department, string jobCode, double rating, double weight, string externalRef, string skill, string country, SeasonalFloorPlanVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string Department { get; set; } = department;
    public string JobCode { get; set; } = jobCode;
    public double Rating { get; set; } = rating;
    public double Weight { get; set; } = weight;
    public string ExternalRef { get; set; } = externalRef;
    public string Skill { get; set; } = skill;
    public string Country { get; set; } = country;
    public SeasonalFloorPlanVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal floor plan variance in status new.</summary>
    public static SeasonalFloorPlanVariance NewSeasonalFloorPlanVariance(string code, string label)
    {
        return new SeasonalFloorPlanVariance(
            code,
            label,
            "",
            "",
            "",
            0.0,
            0.0,
            "",
            "",
            "",
            SeasonalFloorPlanVarianceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalFloorPlanVarianceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalFloorPlanVarianceStatus next)
    {
        switch (Status)
        {
            case SeasonalFloorPlanVarianceStatus.New:
                return next == SeasonalFloorPlanVarianceStatus.Queued || next == SeasonalFloorPlanVarianceStatus.Failed;
            case SeasonalFloorPlanVarianceStatus.Queued:
                switch (next)
                {
                    case SeasonalFloorPlanVarianceStatus.Running:
                        return true;
                }
                return false;
            case SeasonalFloorPlanVarianceStatus.Running:
                return next == SeasonalFloorPlanVarianceStatus.Failed;
            case SeasonalFloorPlanVarianceStatus.Done:
                switch (next)
                {
                    case SeasonalFloorPlanVarianceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalFloorPlanVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal floor plan variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Rating - Weight, 2);
    }

    public SeasonalFloorPlanVariance Copy()
    {
        return new SeasonalFloorPlanVariance(
            Code,
            Label,
            SalaryBand,
            Department,
            JobCode,
            Rating,
            Weight,
            ExternalRef,
            Skill,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
