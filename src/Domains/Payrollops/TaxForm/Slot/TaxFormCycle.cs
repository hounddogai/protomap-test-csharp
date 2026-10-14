namespace Acme.Workforce.Payrollops.TaxForm.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form cycle is in its life cycle.</summary>
public enum TaxFormCycleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A tax form cycle. Its code is unique within a tenant.</summary>
public sealed class TaxFormCycle(string code, string name, double threshold, int tenureMonths, string jobCode, string label, string badge, int startsAt, string summary, List<string> tags, int sequence, TaxFormCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public int TenureMonths { get; set; } = tenureMonths;
    public string JobCode { get; set; } = jobCode;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public int StartsAt { get; set; } = startsAt;
    public string Summary { get; set; } = summary;
    public List<string> Tags { get; set; } = tags;
    public int Sequence { get; set; } = sequence;
    public TaxFormCycleStatus Status { get; set; } = status;

    /// <summary>Starts a tax form cycle in status created.</summary>
    public static TaxFormCycle NewTaxFormCycle(string code, string name)
    {
        return new TaxFormCycle(
            code,
            name,
            0.0,
            0,
            "",
            "",
            "",
            0,
            "",
            new List<string>(),
            0,
            TaxFormCycleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormCycleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormCycleStatus next)
    {
        switch (Status)
        {
            case TaxFormCycleStatus.Created:
                return next == TaxFormCycleStatus.Published || next == TaxFormCycleStatus.Retired;
            case TaxFormCycleStatus.Validated:
                switch (next)
                {
                    case TaxFormCycleStatus.Published:
                    case TaxFormCycleStatus.Retired:
                        return true;
                }
                return false;
            case TaxFormCycleStatus.Published:
                return next == TaxFormCycleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TaxFormCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"tax form cycle"} {Code}";
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

    public double NetLoad()
    {
        return Math.Round(Threshold - (double)TenureMonths, 2);
    }

    public TaxFormCycle Copy()
    {
        return new TaxFormCycle(
            Code,
            Name,
            Threshold,
            TenureMonths,
            JobCode,
            Label,
            Badge,
            StartsAt,
            Summary,
            Tags,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
