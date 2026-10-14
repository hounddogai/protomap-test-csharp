namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form group is in its life cycle.</summary>
public enum TaxFormGroupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tax form group. Its code is unique within a tenant.</summary>
public sealed class TaxFormGroup(string code, string label, double minimum, int sequence, string checksum, double weight, TaxFormGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public int Sequence { get; set; } = sequence;
    public string Checksum { get; set; } = checksum;
    public double Weight { get; set; } = weight;
    public TaxFormGroupStatus Status { get; set; } = status;

    /// <summary>Starts a tax form group in status requested.</summary>
    public static TaxFormGroup NewTaxFormGroup(string code, string label)
    {
        return new TaxFormGroup(code, label, 0.0, 0, "", 0.0, TaxFormGroupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormGroupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormGroupStatus next)
    {
        switch (Status)
        {
            case TaxFormGroupStatus.Requested:
                return next == TaxFormGroupStatus.Granted || next == TaxFormGroupStatus.Expired;
            case TaxFormGroupStatus.Granted:
                return next == TaxFormGroupStatus.Revoked;
            case TaxFormGroupStatus.Revoked:
                return next == TaxFormGroupStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TaxFormGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Weight - (double)Sequence, 2);
    }

    public TaxFormGroup Copy()
    {
        return new TaxFormGroup(Code, Label, Minimum, Sequence, Checksum, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
