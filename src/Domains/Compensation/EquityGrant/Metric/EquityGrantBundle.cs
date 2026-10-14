namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant bundle is in its life cycle.</summary>
public enum EquityGrantBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A equity grant bundle. Its code is unique within a tenant.</summary>
public sealed class EquityGrantBundle(string code, string label, bool approved, int limit, string notes, bool active, int retries, double cost, string salaryBand, string name, EquityGrantBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public int Limit { get; set; } = limit;
    public string Notes { get; set; } = notes;
    public bool Active { get; set; } = active;
    public int Retries { get; set; } = retries;
    public double Cost { get; set; } = cost;
    public string SalaryBand { get; set; } = salaryBand;
    public string Name { get; set; } = name;
    public EquityGrantBundleStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant bundle in status draft.</summary>
    public static EquityGrantBundle NewEquityGrantBundle(string code, string label)
    {
        return new EquityGrantBundle(
            code,
            label,
            false,
            0,
            "",
            false,
            0,
            0.0,
            "",
            "",
            EquityGrantBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantBundleStatus next)
    {
        switch (Status)
        {
            case EquityGrantBundleStatus.Draft:
                return next == EquityGrantBundleStatus.Suspended;
            case EquityGrantBundleStatus.Pending:
                switch (next)
                {
                    case EquityGrantBundleStatus.Active:
                        return true;
                }
                return false;
            case EquityGrantBundleStatus.Active:
                switch (next)
                {
                    case EquityGrantBundleStatus.Suspended:
                    case EquityGrantBundleStatus.Closed:
                        return true;
                }
                return false;
            case EquityGrantBundleStatus.Suspended:
                switch (next)
                {
                    case EquityGrantBundleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"equity grant bundle"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Limit - Cost, 2);
    }

    public EquityGrantBundle Copy()
    {
        return new EquityGrantBundle(
            Code,
            Label,
            Approved,
            Limit,
            Notes,
            Active,
            Retries,
            Cost,
            SalaryBand,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
