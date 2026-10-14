namespace Acme.Workforce.Contractors.StatementOfWork.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default statement of work metric is in its life cycle.</summary>
public enum DefaultStatementOfWorkMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A default statement of work metric as the statement of work team keeps it.</summary>
public sealed class DefaultStatementOfWorkMetric(string code, string name, string country, string phone, bool pinned, string grade, string unit, int quantity, DefaultStatementOfWorkMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Phone { get; set; } = phone;
    public bool Pinned { get; set; } = pinned;
    public string Grade { get; set; } = grade;
    public string Unit { get; set; } = unit;
    public int Quantity { get; set; } = quantity;
    public DefaultStatementOfWorkMetricStatus Status { get; set; } = status;

    /// <summary>Starts a default statement of work metric in status draft.</summary>
    public static DefaultStatementOfWorkMetric NewDefaultStatementOfWorkMetric(string code, string name)
    {
        return new DefaultStatementOfWorkMetric(
            code,
            name,
            "",
            "",
            false,
            "",
            "",
            0,
            DefaultStatementOfWorkMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultStatementOfWorkMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultStatementOfWorkMetricStatus next)
    {
        switch (Status)
        {
            case DefaultStatementOfWorkMetricStatus.Draft:
                switch (next)
                {
                    case DefaultStatementOfWorkMetricStatus.Suspended:
                        return true;
                }
                return false;
            case DefaultStatementOfWorkMetricStatus.Pending:
                return next == DefaultStatementOfWorkMetricStatus.Suspended;
            case DefaultStatementOfWorkMetricStatus.Active:
                return next == DefaultStatementOfWorkMetricStatus.Closed;
            case DefaultStatementOfWorkMetricStatus.Suspended:
                switch (next)
                {
                    case DefaultStatementOfWorkMetricStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultStatementOfWorkMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default statement of work metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default statement of work metric"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public DefaultStatementOfWorkMetric Copy()
    {
        return new DefaultStatementOfWorkMetric(Code, Name, Country, Phone, Pinned, Grade, Unit, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
