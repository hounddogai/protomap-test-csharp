namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form allocation is in its life cycle.</summary>
public enum TaxFormAllocationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A tax form allocation as the tax form team keeps it.</summary>
public sealed class TaxFormAllocation(string code, string label, double ceiling, bool active, int createdAt, int graceDays, double maximum, int leaveDays, int revision, string jobCode, TaxFormAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public bool Active { get; set; } = active;
    public int CreatedAt { get; set; } = createdAt;
    public int GraceDays { get; set; } = graceDays;
    public double Maximum { get; set; } = maximum;
    public int LeaveDays { get; set; } = leaveDays;
    public int Revision { get; set; } = revision;
    public string JobCode { get; set; } = jobCode;
    public TaxFormAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a tax form allocation in status created.</summary>
    public static TaxFormAllocation NewTaxFormAllocation(string code, string label)
    {
        return new TaxFormAllocation(code, label, 0.0, false, 0, 0, 0.0, 0, 0, "", TaxFormAllocationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormAllocationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormAllocationStatus next)
    {
        switch (Status)
        {
            case TaxFormAllocationStatus.Created:
                switch (next)
                {
                    case TaxFormAllocationStatus.Published:
                        return true;
                }
                return false;
            case TaxFormAllocationStatus.Validated:
                switch (next)
                {
                    case TaxFormAllocationStatus.Published:
                        return true;
                }
                return false;
            case TaxFormAllocationStatus.Published:
                return next == TaxFormAllocationStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TaxFormAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetValue()
    {
        return Math.Round((double)GraceDays - (double)LeaveDays, 2);
    }

    public TaxFormAllocation Copy()
    {
        return new TaxFormAllocation(
            Code,
            Label,
            Ceiling,
            Active,
            CreatedAt,
            GraceDays,
            Maximum,
            LeaveDays,
            Revision,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
