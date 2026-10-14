namespace Acme.Workforce.Payrollops.PayRun.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run threshold is in its life cycle.</summary>
public enum PayRunThresholdStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pay run threshold as the pay run team keeps it.</summary>
public sealed class PayRunThreshold(string code, string name, Dictionary<string, string> attributes, string country, bool active, double threshold, double cost, string externalRef, string managerId, string salaryBand, string location, PayRunThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Country { get; set; } = country;
    public bool Active { get; set; } = active;
    public double Threshold { get; set; } = threshold;
    public double Cost { get; set; } = cost;
    public string ExternalRef { get; set; } = externalRef;
    public string ManagerId { get; set; } = managerId;
    public string SalaryBand { get; set; } = salaryBand;
    public string Location { get; set; } = location;
    public PayRunThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a pay run threshold in status new.</summary>
    public static PayRunThreshold NewPayRunThreshold(string code, string name)
    {
        return new PayRunThreshold(
            code,
            name,
            new Dictionary<string, string>(),
            "",
            false,
            0.0,
            0.0,
            "",
            "",
            "",
            "",
            PayRunThresholdStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunThresholdStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunThresholdStatus next)
    {
        switch (Status)
        {
            case PayRunThresholdStatus.New:
                return next == PayRunThresholdStatus.Running || next == PayRunThresholdStatus.Failed;
            case PayRunThresholdStatus.Queued:
                return next == PayRunThresholdStatus.Failed;
            case PayRunThresholdStatus.Running:
                switch (next)
                {
                    case PayRunThresholdStatus.Done:
                    case PayRunThresholdStatus.Failed:
                        return true;
                }
                return false;
            case PayRunThresholdStatus.Done:
                return next == PayRunThresholdStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PayRunThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedValue()
    {
        return Math.Round(Cost * Threshold, 2);
    }

    public PayRunThreshold Copy()
    {
        return new PayRunThreshold(
            Code,
            Name,
            Attributes,
            Country,
            Active,
            Threshold,
            Cost,
            ExternalRef,
            ManagerId,
            SalaryBand,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
