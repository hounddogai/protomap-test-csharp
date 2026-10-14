namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict deduction quota is in its life cycle.</summary>
public enum StrictDeductionQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a strict deduction quota and its current status.</summary>
public sealed class StrictDeductionQuota(string code, string label, double ceiling, int retries, string currency, StrictDeductionQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public int Retries { get; set; } = retries;
    public string Currency { get; set; } = currency;
    public StrictDeductionQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a strict deduction quota in status open.</summary>
    public static StrictDeductionQuota NewStrictDeductionQuota(string code, string label)
    {
        return new StrictDeductionQuota(code, label, 0.0, 0, "", StrictDeductionQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictDeductionQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictDeductionQuotaStatus next)
    {
        switch (Status)
        {
            case StrictDeductionQuotaStatus.Open:
                switch (next)
                {
                    case StrictDeductionQuotaStatus.InReview:
                    case StrictDeductionQuotaStatus.Resolved:
                        return true;
                }
                return false;
            case StrictDeductionQuotaStatus.InReview:
                return next == StrictDeductionQuotaStatus.Resolved || next == StrictDeductionQuotaStatus.Archived;
            case StrictDeductionQuotaStatus.Resolved:
                switch (next)
                {
                    case StrictDeductionQuotaStatus.Reopened:
                    case StrictDeductionQuotaStatus.Archived:
                        return true;
                }
                return false;
            case StrictDeductionQuotaStatus.Reopened:
                return next == StrictDeductionQuotaStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictDeductionQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict deduction quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Ceiling - (double)Retries, 2);
    }

    public StrictDeductionQuota Copy()
    {
        return new StrictDeductionQuota(Code, Label, Ceiling, Retries, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
