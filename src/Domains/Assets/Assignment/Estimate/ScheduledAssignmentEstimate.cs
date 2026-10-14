namespace Acme.Workforce.Assets.Assignment.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled assignment estimate is in its life cycle.</summary>
public enum ScheduledAssignmentEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a scheduled assignment estimate and its current status.</summary>
public sealed class ScheduledAssignmentEstimate(string code, string label, string salaryBand, int retries, int revision, int tenureMonths, string skill, ScheduledAssignmentEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public int Retries { get; set; } = retries;
    public int Revision { get; set; } = revision;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Skill { get; set; } = skill;
    public ScheduledAssignmentEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled assignment estimate in status draft.</summary>
    public static ScheduledAssignmentEstimate NewScheduledAssignmentEstimate(string code, string label)
    {
        return new ScheduledAssignmentEstimate(
            code,
            label,
            "",
            0,
            0,
            0,
            "",
            ScheduledAssignmentEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAssignmentEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAssignmentEstimateStatus next)
    {
        switch (Status)
        {
            case ScheduledAssignmentEstimateStatus.Draft:
                return next == ScheduledAssignmentEstimateStatus.Active || next == ScheduledAssignmentEstimateStatus.Closed;
            case ScheduledAssignmentEstimateStatus.Pending:
                return next == ScheduledAssignmentEstimateStatus.Active;
            case ScheduledAssignmentEstimateStatus.Active:
                switch (next)
                {
                    case ScheduledAssignmentEstimateStatus.Suspended:
                    case ScheduledAssignmentEstimateStatus.Closed:
                        return true;
                }
                return false;
            case ScheduledAssignmentEstimateStatus.Suspended:
                switch (next)
                {
                    case ScheduledAssignmentEstimateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledAssignmentEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled assignment estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Retries - (double)TenureMonths, 2);
    }

    public ScheduledAssignmentEstimate Copy()
    {
        return new ScheduledAssignmentEstimate(
            Code,
            Label,
            SalaryBand,
            Retries,
            Revision,
            TenureMonths,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
