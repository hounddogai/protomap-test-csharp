namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default payslip threshold is in its life cycle.</summary>
public enum DefaultPayslipThresholdStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One default payslip threshold, with what the planners need to act on it.</summary>
public sealed class DefaultPayslipThreshold(string code, string title, string country, string unit, string checksum, List<string> tags, double amount, string externalRef, DefaultPayslipThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public string Unit { get; set; } = unit;
    public string Checksum { get; set; } = checksum;
    public List<string> Tags { get; set; } = tags;
    public double Amount { get; set; } = amount;
    public string ExternalRef { get; set; } = externalRef;
    public DefaultPayslipThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a default payslip threshold in status new.</summary>
    public static DefaultPayslipThreshold NewDefaultPayslipThreshold(string code, string title)
    {
        return new DefaultPayslipThreshold(
            code,
            title,
            "",
            "",
            "",
            new List<string>(),
            0.0,
            "",
            DefaultPayslipThresholdStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPayslipThresholdStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPayslipThresholdStatus next)
    {
        switch (Status)
        {
            case DefaultPayslipThresholdStatus.New:
                return next == DefaultPayslipThresholdStatus.Queued;
            case DefaultPayslipThresholdStatus.Queued:
                return next == DefaultPayslipThresholdStatus.Running;
            case DefaultPayslipThresholdStatus.Running:
                switch (next)
                {
                    case DefaultPayslipThresholdStatus.Done:
                    case DefaultPayslipThresholdStatus.Failed:
                        return true;
                }
                return false;
            case DefaultPayslipThresholdStatus.Done:
                return next == DefaultPayslipThresholdStatus.Failed;
        }
        return false;
    }

    public void MoveTo(DefaultPayslipThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default payslip threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default payslip threshold"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public DefaultPayslipThreshold Copy()
    {
        return new DefaultPayslipThreshold(Code, Title, Country, Unit, Checksum, Tags, Amount, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
