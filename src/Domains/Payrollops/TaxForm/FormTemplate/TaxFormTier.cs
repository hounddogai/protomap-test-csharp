namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form tier is in its life cycle.</summary>
public enum TaxFormTierStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A tax form tier as the tax form team keeps it.</summary>
public sealed class TaxFormTier(string code, string name, string contactEmail, string reviewer, int batchSize, string segment, double amount, TaxFormTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public string Reviewer { get; set; } = reviewer;
    public int BatchSize { get; set; } = batchSize;
    public string Segment { get; set; } = segment;
    public double Amount { get; set; } = amount;
    public TaxFormTierStatus Status { get; set; } = status;

    /// <summary>Starts a tax form tier in status new.</summary>
    public static TaxFormTier NewTaxFormTier(string code, string name)
    {
        return new TaxFormTier(code, name, "", "", 0, "", 0.0, TaxFormTierStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormTierStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormTierStatus next)
    {
        switch (Status)
        {
            case TaxFormTierStatus.New:
                return next == TaxFormTierStatus.Running || next == TaxFormTierStatus.Done;
            case TaxFormTierStatus.Queued:
                return next == TaxFormTierStatus.Failed;
            case TaxFormTierStatus.Running:
                switch (next)
                {
                    case TaxFormTierStatus.Failed:
                        return true;
                }
                return false;
            case TaxFormTierStatus.Done:
                switch (next)
                {
                    case TaxFormTierStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tax form tier"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Amount - (double)BatchSize, 2);
    }

    public TaxFormTier Copy()
    {
        return new TaxFormTier(Code, Name, ContactEmail, Reviewer, BatchSize, Segment, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
