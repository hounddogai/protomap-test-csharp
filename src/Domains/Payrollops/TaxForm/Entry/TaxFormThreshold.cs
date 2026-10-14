namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form threshold is in its life cycle.</summary>
public enum TaxFormThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tax form threshold. Its code is unique within a tenant.</summary>
public sealed class TaxFormThreshold(string code, string label, int quantity, int sequence, string description, string notes, TaxFormThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public int Sequence { get; set; } = sequence;
    public string Description { get; set; } = description;
    public string Notes { get; set; } = notes;
    public TaxFormThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a tax form threshold in status draft.</summary>
    public static TaxFormThreshold NewTaxFormThreshold(string code, string label)
    {
        return new TaxFormThreshold(code, label, 0, 0, "", "", TaxFormThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormThresholdStatus next)
    {
        switch (Status)
        {
            case TaxFormThresholdStatus.Draft:
                return next == TaxFormThresholdStatus.Pending || next == TaxFormThresholdStatus.Suspended;
            case TaxFormThresholdStatus.Pending:
                switch (next)
                {
                    case TaxFormThresholdStatus.Active:
                        return true;
                }
                return false;
            case TaxFormThresholdStatus.Active:
                switch (next)
                {
                    case TaxFormThresholdStatus.Suspended:
                    case TaxFormThresholdStatus.Closed:
                        return true;
                }
                return false;
            case TaxFormThresholdStatus.Suspended:
                return next == TaxFormThresholdStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TaxFormThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"tax form threshold"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Quantity * (double)Sequence, 2);
    }

    public TaxFormThreshold Copy()
    {
        return new TaxFormThreshold(Code, Label, Quantity, Sequence, Description, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
