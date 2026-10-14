namespace Acme.Workforce.Payrollops.Garnishment.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment estimate is in its life cycle.</summary>
public enum GarnishmentEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A garnishment estimate. Its code is unique within a tenant.</summary>
public sealed class GarnishmentEstimate(string code, string name, string notes, string checksum, string label, int quantity, GarnishmentEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public string Checksum { get; set; } = checksum;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public GarnishmentEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment estimate in status planned.</summary>
    public static GarnishmentEstimate NewGarnishmentEstimate(string code, string name)
    {
        return new GarnishmentEstimate(code, name, "", "", "", 0, GarnishmentEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentEstimateStatus next)
    {
        switch (Status)
        {
            case GarnishmentEstimateStatus.Planned:
                switch (next)
                {
                    case GarnishmentEstimateStatus.Scheduled:
                        return true;
                }
                return false;
            case GarnishmentEstimateStatus.Scheduled:
                return next == GarnishmentEstimateStatus.Cancelled;
            case GarnishmentEstimateStatus.InProgress:
                switch (next)
                {
                    case GarnishmentEstimateStatus.Completed:
                        return true;
                }
                return false;
            case GarnishmentEstimateStatus.Completed:
                return next == GarnishmentEstimateStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(GarnishmentEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public GarnishmentEstimate Copy()
    {
        return new GarnishmentEstimate(Code, Name, Notes, Checksum, Label, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
