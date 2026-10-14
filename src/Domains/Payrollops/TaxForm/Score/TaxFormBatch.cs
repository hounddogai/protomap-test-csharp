namespace Acme.Workforce.Payrollops.TaxForm.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form batch is in its life cycle.</summary>
public enum TaxFormBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A tax form batch as the tax form team keeps it.</summary>
public sealed class TaxFormBatch(string code, string label, int tenureMonths, int endsAt, string reviewer, bool pinned, string summary, TaxFormBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public int EndsAt { get; set; } = endsAt;
    public string Reviewer { get; set; } = reviewer;
    public bool Pinned { get; set; } = pinned;
    public string Summary { get; set; } = summary;
    public TaxFormBatchStatus Status { get; set; } = status;

    /// <summary>Starts a tax form batch in status created.</summary>
    public static TaxFormBatch NewTaxFormBatch(string code, string label)
    {
        return new TaxFormBatch(code, label, 0, 0, "", false, "", TaxFormBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormBatchStatus next)
    {
        switch (Status)
        {
            case TaxFormBatchStatus.Created:
                switch (next)
                {
                    case TaxFormBatchStatus.Validated:
                        return true;
                }
                return false;
            case TaxFormBatchStatus.Validated:
                switch (next)
                {
                    case TaxFormBatchStatus.Retired:
                        return true;
                }
                return false;
            case TaxFormBatchStatus.Published:
                switch (next)
                {
                    case TaxFormBatchStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)EndsAt * (double)TenureMonths, 2);
    }

    public TaxFormBatch Copy()
    {
        return new TaxFormBatch(Code, Label, TenureMonths, EndsAt, Reviewer, Pinned, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
