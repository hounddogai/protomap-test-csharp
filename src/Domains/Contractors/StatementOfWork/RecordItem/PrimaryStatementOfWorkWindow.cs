namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary statement of work window is in its life cycle.</summary>
public enum PrimaryStatementOfWorkWindowStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A primary statement of work window. Its code is unique within a tenant.</summary>
public sealed class PrimaryStatementOfWorkWindow(string code, string title, double amount, string contactEmail, double cost, string costCenter, string source, string salaryBand, PrimaryStatementOfWorkWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public string ContactEmail { get; set; } = contactEmail;
    public double Cost { get; set; } = cost;
    public string CostCenter { get; set; } = costCenter;
    public string Source { get; set; } = source;
    public string SalaryBand { get; set; } = salaryBand;
    public PrimaryStatementOfWorkWindowStatus Status { get; set; } = status;

    /// <summary>Starts a primary statement of work window in status planned.</summary>
    public static PrimaryStatementOfWorkWindow NewPrimaryStatementOfWorkWindow(string code, string title)
    {
        return new PrimaryStatementOfWorkWindow(
            code,
            title,
            0.0,
            "",
            0.0,
            "",
            "",
            "",
            PrimaryStatementOfWorkWindowStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryStatementOfWorkWindowStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryStatementOfWorkWindowStatus next)
    {
        switch (Status)
        {
            case PrimaryStatementOfWorkWindowStatus.Planned:
                switch (next)
                {
                    case PrimaryStatementOfWorkWindowStatus.Completed:
                    case PrimaryStatementOfWorkWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryStatementOfWorkWindowStatus.Scheduled:
                switch (next)
                {
                    case PrimaryStatementOfWorkWindowStatus.InProgress:
                    case PrimaryStatementOfWorkWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryStatementOfWorkWindowStatus.InProgress:
                return next == PrimaryStatementOfWorkWindowStatus.Completed || next == PrimaryStatementOfWorkWindowStatus.Cancelled;
            case PrimaryStatementOfWorkWindowStatus.Completed:
                switch (next)
                {
                    case PrimaryStatementOfWorkWindowStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryStatementOfWorkWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary statement of work window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Cost - Amount, 2);
    }

    public PrimaryStatementOfWorkWindow Copy()
    {
        return new PrimaryStatementOfWorkWindow(
            Code,
            Title,
            Amount,
            ContactEmail,
            Cost,
            CostCenter,
            Source,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
