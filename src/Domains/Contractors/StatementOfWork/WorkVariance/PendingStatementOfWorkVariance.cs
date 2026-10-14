namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending statement of work variance is in its life cycle.</summary>
public enum PendingStatementOfWorkVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pending statement of work variance. Its code is unique within a tenant.</summary>
public sealed class PendingStatementOfWorkVariance(string code, string title, double ceiling, string managerId, string channel, string department, string country, PendingStatementOfWorkVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public string ManagerId { get; set; } = managerId;
    public string Channel { get; set; } = channel;
    public string Department { get; set; } = department;
    public string Country { get; set; } = country;
    public PendingStatementOfWorkVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a pending statement of work variance in status draft.</summary>
    public static PendingStatementOfWorkVariance NewPendingStatementOfWorkVariance(string code, string title)
    {
        return new PendingStatementOfWorkVariance(
            code,
            title,
            0.0,
            "",
            "",
            "",
            "",
            PendingStatementOfWorkVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingStatementOfWorkVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingStatementOfWorkVarianceStatus next)
    {
        switch (Status)
        {
            case PendingStatementOfWorkVarianceStatus.Draft:
                switch (next)
                {
                    case PendingStatementOfWorkVarianceStatus.Pending:
                        return true;
                }
                return false;
            case PendingStatementOfWorkVarianceStatus.Pending:
                return next == PendingStatementOfWorkVarianceStatus.Suspended || next == PendingStatementOfWorkVarianceStatus.Closed;
            case PendingStatementOfWorkVarianceStatus.Active:
                return next == PendingStatementOfWorkVarianceStatus.Suspended;
            case PendingStatementOfWorkVarianceStatus.Suspended:
                return next == PendingStatementOfWorkVarianceStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PendingStatementOfWorkVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending statement of work variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending statement of work variance"} {Code}";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public PendingStatementOfWorkVariance Copy()
    {
        return new PendingStatementOfWorkVariance(
            Code,
            Title,
            Ceiling,
            ManagerId,
            Channel,
            Department,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
