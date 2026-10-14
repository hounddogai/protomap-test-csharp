namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched warranty agreement is in its life cycle.</summary>
public enum BatchedWarrantyAgreementStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A batched warranty agreement. Its code is unique within a tenant.</summary>
public sealed class BatchedWarrantyAgreement(string code, string name, double weight, int leaveDays, string title, string locale, bool visible, string summary, string phone, bool archived, BatchedWarrantyAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public int LeaveDays { get; set; } = leaveDays;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public bool Visible { get; set; } = visible;
    public string Summary { get; set; } = summary;
    public string Phone { get; set; } = phone;
    public bool Archived { get; set; } = archived;
    public BatchedWarrantyAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a batched warranty agreement in status new.</summary>
    public static BatchedWarrantyAgreement NewBatchedWarrantyAgreement(string code, string name)
    {
        return new BatchedWarrantyAgreement(
            code,
            name,
            0.0,
            0,
            "",
            "",
            false,
            "",
            "",
            false,
            BatchedWarrantyAgreementStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedWarrantyAgreementStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedWarrantyAgreementStatus next)
    {
        switch (Status)
        {
            case BatchedWarrantyAgreementStatus.New:
                switch (next)
                {
                    case BatchedWarrantyAgreementStatus.Queued:
                        return true;
                }
                return false;
            case BatchedWarrantyAgreementStatus.Queued:
                return next == BatchedWarrantyAgreementStatus.Done;
            case BatchedWarrantyAgreementStatus.Running:
                return next == BatchedWarrantyAgreementStatus.Done || next == BatchedWarrantyAgreementStatus.Failed;
            case BatchedWarrantyAgreementStatus.Done:
                switch (next)
                {
                    case BatchedWarrantyAgreementStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedWarrantyAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched warranty agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)LeaveDays * Weight, 2);
    }

    public BatchedWarrantyAgreement Copy()
    {
        return new BatchedWarrantyAgreement(
            Code,
            Name,
            Weight,
            LeaveDays,
            Title,
            Locale,
            Visible,
            Summary,
            Phone,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
