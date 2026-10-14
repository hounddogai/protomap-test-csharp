namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual statement of work baseline is in its life cycle.</summary>
public enum ManualStatementOfWorkBaselineStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A manual statement of work baseline as the statement of work team keeps it.</summary>
public sealed class ManualStatementOfWorkBaseline(string code, string title, string reviewer, double maximum, int graceDays, string contactEmail, int capacity, string unit, string category, string managerId, int durationMinutes, ManualStatementOfWorkBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public double Maximum { get; set; } = maximum;
    public int GraceDays { get; set; } = graceDays;
    public string ContactEmail { get; set; } = contactEmail;
    public int Capacity { get; set; } = capacity;
    public string Unit { get; set; } = unit;
    public string Category { get; set; } = category;
    public string ManagerId { get; set; } = managerId;
    public int DurationMinutes { get; set; } = durationMinutes;
    public ManualStatementOfWorkBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a manual statement of work baseline in status draft.</summary>
    public static ManualStatementOfWorkBaseline NewManualStatementOfWorkBaseline(string code, string title)
    {
        return new ManualStatementOfWorkBaseline(
            code,
            title,
            "",
            0.0,
            0,
            "",
            0,
            "",
            "",
            "",
            0,
            ManualStatementOfWorkBaselineStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualStatementOfWorkBaselineStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualStatementOfWorkBaselineStatus next)
    {
        switch (Status)
        {
            case ManualStatementOfWorkBaselineStatus.Draft:
                switch (next)
                {
                    case ManualStatementOfWorkBaselineStatus.Suspended:
                    case ManualStatementOfWorkBaselineStatus.Closed:
                        return true;
                }
                return false;
            case ManualStatementOfWorkBaselineStatus.Pending:
                return next == ManualStatementOfWorkBaselineStatus.Suspended;
            case ManualStatementOfWorkBaselineStatus.Active:
                return next == ManualStatementOfWorkBaselineStatus.Suspended || next == ManualStatementOfWorkBaselineStatus.Closed;
            case ManualStatementOfWorkBaselineStatus.Suspended:
                switch (next)
                {
                    case ManualStatementOfWorkBaselineStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualStatementOfWorkBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual statement of work baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)DurationMinutes - (double)GraceDays, 2);
    }

    public ManualStatementOfWorkBaseline Copy()
    {
        return new ManualStatementOfWorkBaseline(
            Code,
            Title,
            Reviewer,
            Maximum,
            GraceDays,
            ContactEmail,
            Capacity,
            Unit,
            Category,
            ManagerId,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
