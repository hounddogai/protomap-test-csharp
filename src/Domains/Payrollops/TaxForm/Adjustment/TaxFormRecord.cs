namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form record is in its life cycle.</summary>
public enum TaxFormRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tax form record as the tax form team keeps it.</summary>
public sealed class TaxFormRecord(string code, string label, string contactEmail, double weight, int sequence, double fte, double floorValue, string managerId, TaxFormRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public double Weight { get; set; } = weight;
    public int Sequence { get; set; } = sequence;
    public double Fte { get; set; } = fte;
    public double FloorValue { get; set; } = floorValue;
    public string ManagerId { get; set; } = managerId;
    public TaxFormRecordStatus Status { get; set; } = status;

    /// <summary>Starts a tax form record in status planned.</summary>
    public static TaxFormRecord NewTaxFormRecord(string code, string label)
    {
        return new TaxFormRecord(code, label, "", 0.0, 0, 0.0, 0.0, "", TaxFormRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormRecordStatus next)
    {
        switch (Status)
        {
            case TaxFormRecordStatus.Planned:
                return next == TaxFormRecordStatus.InProgress || next == TaxFormRecordStatus.Cancelled;
            case TaxFormRecordStatus.Scheduled:
                return next == TaxFormRecordStatus.InProgress || next == TaxFormRecordStatus.Cancelled;
            case TaxFormRecordStatus.InProgress:
                return next == TaxFormRecordStatus.Completed || next == TaxFormRecordStatus.Cancelled;
            case TaxFormRecordStatus.Completed:
                switch (next)
                {
                    case TaxFormRecordStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"tax form record"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(Fte * Weight, 2);
    }

    public TaxFormRecord Copy()
    {
        return new TaxFormRecord(Code, Label, ContactEmail, Weight, Sequence, Fte, FloorValue, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
