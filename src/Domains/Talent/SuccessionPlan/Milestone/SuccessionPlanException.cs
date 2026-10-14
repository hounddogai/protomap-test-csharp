namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan exception is in its life cycle.</summary>
public enum SuccessionPlanExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a succession plan exception and its current status.</summary>
public sealed class SuccessionPlanException(string code, string title, double cost, double weight, string grade, string team, int createdAt, string salaryBand, int leaveDays, string unit, string jobCode, SuccessionPlanExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public double Weight { get; set; } = weight;
    public string Grade { get; set; } = grade;
    public string Team { get; set; } = team;
    public int CreatedAt { get; set; } = createdAt;
    public string SalaryBand { get; set; } = salaryBand;
    public int LeaveDays { get; set; } = leaveDays;
    public string Unit { get; set; } = unit;
    public string JobCode { get; set; } = jobCode;
    public SuccessionPlanExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan exception in status draft.</summary>
    public static SuccessionPlanException NewSuccessionPlanException(string code, string title)
    {
        return new SuccessionPlanException(
            code,
            title,
            0.0,
            0.0,
            "",
            "",
            0,
            "",
            0,
            "",
            "",
            SuccessionPlanExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanExceptionStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanExceptionStatus.Draft:
                return next == SuccessionPlanExceptionStatus.Suspended || next == SuccessionPlanExceptionStatus.Closed;
            case SuccessionPlanExceptionStatus.Pending:
                switch (next)
                {
                    case SuccessionPlanExceptionStatus.Suspended:
                    case SuccessionPlanExceptionStatus.Closed:
                        return true;
                }
                return false;
            case SuccessionPlanExceptionStatus.Active:
                return next == SuccessionPlanExceptionStatus.Suspended;
            case SuccessionPlanExceptionStatus.Suspended:
                switch (next)
                {
                    case SuccessionPlanExceptionStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)LeaveDays * (double)CreatedAt, 2);
    }

    public SuccessionPlanException Copy()
    {
        return new SuccessionPlanException(
            Code,
            Title,
            Cost,
            Weight,
            Grade,
            Team,
            CreatedAt,
            SalaryBand,
            LeaveDays,
            Unit,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
