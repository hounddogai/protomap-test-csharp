namespace Acme.Workforce.Workplace.AccessBadge.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge batch is in its life cycle.</summary>
public enum AccessBadgeBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A access badge batch as the access badge team keeps it.</summary>
public sealed class AccessBadgeBatch(string code, string name, string salaryBand, double rate, string channel, string badge, int graceDays, int tenureMonths, string employeeId, AccessBadgeBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public double Rate { get; set; } = rate;
    public string Channel { get; set; } = channel;
    public string Badge { get; set; } = badge;
    public int GraceDays { get; set; } = graceDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public string EmployeeId { get; set; } = employeeId;
    public AccessBadgeBatchStatus Status { get; set; } = status;

    /// <summary>Starts a access badge batch in status open.</summary>
    public static AccessBadgeBatch NewAccessBadgeBatch(string code, string name)
    {
        return new AccessBadgeBatch(code, name, "", 0.0, "", "", 0, 0, "", AccessBadgeBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeBatchStatus next)
    {
        switch (Status)
        {
            case AccessBadgeBatchStatus.Open:
                switch (next)
                {
                    case AccessBadgeBatchStatus.InReview:
                        return true;
                }
                return false;
            case AccessBadgeBatchStatus.InReview:
                switch (next)
                {
                    case AccessBadgeBatchStatus.Resolved:
                    case AccessBadgeBatchStatus.Reopened:
                        return true;
                }
                return false;
            case AccessBadgeBatchStatus.Resolved:
                switch (next)
                {
                    case AccessBadgeBatchStatus.Reopened:
                    case AccessBadgeBatchStatus.Archived:
                        return true;
                }
                return false;
            case AccessBadgeBatchStatus.Reopened:
                switch (next)
                {
                    case AccessBadgeBatchStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetValue()
    {
        return Math.Round((double)GraceDays + Rate, 2);
    }

    public AccessBadgeBatch Copy()
    {
        return new AccessBadgeBatch(
            Code,
            Name,
            SalaryBand,
            Rate,
            Channel,
            Badge,
            GraceDays,
            TenureMonths,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
