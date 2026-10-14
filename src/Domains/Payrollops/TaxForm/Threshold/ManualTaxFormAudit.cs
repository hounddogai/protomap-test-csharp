namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual tax form audit is in its life cycle.</summary>
public enum ManualTaxFormAuditStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One manual tax form audit, with what the planners need to act on it.</summary>
public sealed class ManualTaxFormAudit(string code, string name, double rating, string title, int capacity, string label, string employeeId, string channel, string currency, string country, string segment, ManualTaxFormAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public string Channel { get; set; } = channel;
    public string Currency { get; set; } = currency;
    public string Country { get; set; } = country;
    public string Segment { get; set; } = segment;
    public ManualTaxFormAuditStatus Status { get; set; } = status;

    /// <summary>Starts a manual tax form audit in status idle.</summary>
    public static ManualTaxFormAudit NewManualTaxFormAudit(string code, string name)
    {
        return new ManualTaxFormAudit(code, name, 0.0, "", 0, "", "", "", "", "", "", ManualTaxFormAuditStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualTaxFormAuditStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualTaxFormAuditStatus next)
    {
        switch (Status)
        {
            case ManualTaxFormAuditStatus.Idle:
                return next == ManualTaxFormAuditStatus.Warming || next == ManualTaxFormAuditStatus.Stopped;
            case ManualTaxFormAuditStatus.Warming:
                return next == ManualTaxFormAuditStatus.Stopped;
            case ManualTaxFormAuditStatus.Ready:
                return next == ManualTaxFormAuditStatus.Stopped;
            case ManualTaxFormAuditStatus.Draining:
                switch (next)
                {
                    case ManualTaxFormAuditStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualTaxFormAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual tax form audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Capacity - Rating, 2);
    }

    public ManualTaxFormAudit Copy()
    {
        return new ManualTaxFormAudit(
            Code,
            Name,
            Rating,
            Title,
            Capacity,
            Label,
            EmployeeId,
            Channel,
            Currency,
            Country,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
