namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal tax form allocation is in its life cycle.</summary>
public enum SeasonalTaxFormAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A seasonal tax form allocation. Its code is unique within a tenant.</summary>
public sealed class SeasonalTaxFormAllocation(string code, string name, int capacity, int tenureMonths, string department, List<string> tags, double hours, SeasonalTaxFormAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Department { get; set; } = department;
    public List<string> Tags { get; set; } = tags;
    public double Hours { get; set; } = hours;
    public SeasonalTaxFormAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal tax form allocation in status idle.</summary>
    public static SeasonalTaxFormAllocation NewSeasonalTaxFormAllocation(string code, string name)
    {
        return new SeasonalTaxFormAllocation(
            code,
            name,
            0,
            0,
            "",
            new List<string>(),
            0.0,
            SeasonalTaxFormAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalTaxFormAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalTaxFormAllocationStatus next)
    {
        switch (Status)
        {
            case SeasonalTaxFormAllocationStatus.Idle:
                switch (next)
                {
                    case SeasonalTaxFormAllocationStatus.Warming:
                    case SeasonalTaxFormAllocationStatus.Ready:
                        return true;
                }
                return false;
            case SeasonalTaxFormAllocationStatus.Warming:
                return next == SeasonalTaxFormAllocationStatus.Stopped;
            case SeasonalTaxFormAllocationStatus.Ready:
                switch (next)
                {
                    case SeasonalTaxFormAllocationStatus.Stopped:
                        return true;
                }
                return false;
            case SeasonalTaxFormAllocationStatus.Draining:
                switch (next)
                {
                    case SeasonalTaxFormAllocationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalTaxFormAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal tax form allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double EffectiveTotal()
    {
        return Math.Round((double)TenureMonths + Hours, 2);
    }

    public SeasonalTaxFormAllocation Copy()
    {
        return new SeasonalTaxFormAllocation(Code, Name, Capacity, TenureMonths, Department, Tags, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
