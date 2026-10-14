namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped payslip batch is in its life cycle.</summary>
public enum CappedPayslipBatchStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A capped payslip batch. Its code is unique within a tenant.</summary>
public sealed class CappedPayslipBatch(string code, string label, string externalRef, int updatedAt, int startsAt, List<string> tags, double discount, string contactEmail, string locale, CappedPayslipBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public int UpdatedAt { get; set; } = updatedAt;
    public int StartsAt { get; set; } = startsAt;
    public List<string> Tags { get; set; } = tags;
    public double Discount { get; set; } = discount;
    public string ContactEmail { get; set; } = contactEmail;
    public string Locale { get; set; } = locale;
    public CappedPayslipBatchStatus Status { get; set; } = status;

    /// <summary>Starts a capped payslip batch in status idle.</summary>
    public static CappedPayslipBatch NewCappedPayslipBatch(string code, string label)
    {
        return new CappedPayslipBatch(
            code,
            label,
            "",
            0,
            0,
            new List<string>(),
            0.0,
            "",
            "",
            CappedPayslipBatchStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPayslipBatchStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPayslipBatchStatus next)
    {
        switch (Status)
        {
            case CappedPayslipBatchStatus.Idle:
                return next == CappedPayslipBatchStatus.Draining;
            case CappedPayslipBatchStatus.Warming:
                switch (next)
                {
                    case CappedPayslipBatchStatus.Draining:
                        return true;
                }
                return false;
            case CappedPayslipBatchStatus.Ready:
                return next == CappedPayslipBatchStatus.Draining || next == CappedPayslipBatchStatus.Stopped;
            case CappedPayslipBatchStatus.Draining:
                switch (next)
                {
                    case CappedPayslipBatchStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedPayslipBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped payslip batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedValue()
    {
        return Math.Round(Discount - (double)UpdatedAt, 2);
    }

    public CappedPayslipBatch Copy()
    {
        return new CappedPayslipBatch(
            Code,
            Label,
            ExternalRef,
            UpdatedAt,
            StartsAt,
            Tags,
            Discount,
            ContactEmail,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
