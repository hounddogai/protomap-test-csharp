namespace Acme.Workforce.Compliance.Exception.ExceptionAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception variance is in its life cycle.</summary>
public enum ExceptionVarianceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A exception variance as the exception team keeps it.</summary>
public sealed class ExceptionVariance(string code, string title, string description, double discount, string currency, string location, string skill, int limit, int retries, ExceptionVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public double Discount { get; set; } = discount;
    public string Currency { get; set; } = currency;
    public string Location { get; set; } = location;
    public string Skill { get; set; } = skill;
    public int Limit { get; set; } = limit;
    public int Retries { get; set; } = retries;
    public ExceptionVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a exception variance in status proposed.</summary>
    public static ExceptionVariance NewExceptionVariance(string code, string title)
    {
        return new ExceptionVariance(code, title, "", 0.0, "", "", "", 0, 0, ExceptionVarianceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionVarianceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionVarianceStatus next)
    {
        switch (Status)
        {
            case ExceptionVarianceStatus.Proposed:
                return next == ExceptionVarianceStatus.Approved;
            case ExceptionVarianceStatus.Approved:
                switch (next)
                {
                    case ExceptionVarianceStatus.Withdrawn:
                        return true;
                }
                return false;
            case ExceptionVarianceStatus.Rejected:
                return next == ExceptionVarianceStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ExceptionVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"exception variance"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Retries * Discount, 2);
    }

    public ExceptionVariance Copy()
    {
        return new ExceptionVariance(
            Code,
            Title,
            Description,
            Discount,
            Currency,
            Location,
            Skill,
            Limit,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
