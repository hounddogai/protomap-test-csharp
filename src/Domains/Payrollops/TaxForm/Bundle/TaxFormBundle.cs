namespace Acme.Workforce.Payrollops.TaxForm.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form bundle is in its life cycle.</summary>
public enum TaxFormBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a tax form bundle and its current status.</summary>
public sealed class TaxFormBundle(string code, string name, string label, double margin, double hours, string badge, string ownerId, string salaryBand, string notes, int limit, TaxFormBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public double Hours { get; set; } = hours;
    public string Badge { get; set; } = badge;
    public string OwnerId { get; set; } = ownerId;
    public string SalaryBand { get; set; } = salaryBand;
    public string Notes { get; set; } = notes;
    public int Limit { get; set; } = limit;
    public TaxFormBundleStatus Status { get; set; } = status;

    /// <summary>Starts a tax form bundle in status draft.</summary>
    public static TaxFormBundle NewTaxFormBundle(string code, string name)
    {
        return new TaxFormBundle(code, name, "", 0.0, 0.0, "", "", "", "", 0, TaxFormBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormBundleStatus next)
    {
        switch (Status)
        {
            case TaxFormBundleStatus.Draft:
                return next == TaxFormBundleStatus.Pending || next == TaxFormBundleStatus.Closed;
            case TaxFormBundleStatus.Pending:
                return next == TaxFormBundleStatus.Active || next == TaxFormBundleStatus.Suspended;
            case TaxFormBundleStatus.Active:
                return next == TaxFormBundleStatus.Suspended;
            case TaxFormBundleStatus.Suspended:
                switch (next)
                {
                    case TaxFormBundleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Hours + (double)Limit, 2);
    }

    public TaxFormBundle Copy()
    {
        return new TaxFormBundle(Code, Name, Label, Margin, Hours, Badge, OwnerId, SalaryBand, Notes, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
