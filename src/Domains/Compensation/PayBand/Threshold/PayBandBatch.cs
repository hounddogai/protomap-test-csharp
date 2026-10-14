namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band batch is in its life cycle.</summary>
public enum PayBandBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pay band batch and its current status.</summary>
public sealed class PayBandBatch(string code, string label, double ratio, int capacity, string currency, string country, PayBandBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public int Capacity { get; set; } = capacity;
    public string Currency { get; set; } = currency;
    public string Country { get; set; } = country;
    public PayBandBatchStatus Status { get; set; } = status;

    /// <summary>Starts a pay band batch in status created.</summary>
    public static PayBandBatch NewPayBandBatch(string code, string label)
    {
        return new PayBandBatch(code, label, 0.0, 0, "", "", PayBandBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandBatchStatus next)
    {
        switch (Status)
        {
            case PayBandBatchStatus.Created:
                switch (next)
                {
                    case PayBandBatchStatus.Validated:
                        return true;
                }
                return false;
            case PayBandBatchStatus.Validated:
                switch (next)
                {
                    case PayBandBatchStatus.Published:
                    case PayBandBatchStatus.Retired:
                        return true;
                }
                return false;
            case PayBandBatchStatus.Published:
                switch (next)
                {
                    case PayBandBatchStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Ratio * (double)Capacity, 2);
    }

    public PayBandBatch Copy()
    {
        return new PayBandBatch(Code, Label, Ratio, Capacity, Currency, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
