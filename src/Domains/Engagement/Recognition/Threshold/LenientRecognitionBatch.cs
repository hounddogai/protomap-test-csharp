namespace Acme.Workforce.Engagement.Recognition.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient recognition batch is in its life cycle.</summary>
public enum LenientRecognitionBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient recognition batch. Its code is unique within a tenant.</summary>
public sealed class LenientRecognitionBatch(string code, string label, string contactEmail, bool pinned, string unit, string team, string department, double discount, LenientRecognitionBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Pinned { get; set; } = pinned;
    public string Unit { get; set; } = unit;
    public string Team { get; set; } = team;
    public string Department { get; set; } = department;
    public double Discount { get; set; } = discount;
    public LenientRecognitionBatchStatus Status { get; set; } = status;

    /// <summary>Starts a lenient recognition batch in status created.</summary>
    public static LenientRecognitionBatch NewLenientRecognitionBatch(string code, string label)
    {
        return new LenientRecognitionBatch(
            code,
            label,
            "",
            false,
            "",
            "",
            "",
            0.0,
            LenientRecognitionBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientRecognitionBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientRecognitionBatchStatus next)
    {
        switch (Status)
        {
            case LenientRecognitionBatchStatus.Created:
                return next == LenientRecognitionBatchStatus.Retired;
            case LenientRecognitionBatchStatus.Validated:
                switch (next)
                {
                    case LenientRecognitionBatchStatus.Retired:
                        return true;
                }
                return false;
            case LenientRecognitionBatchStatus.Published:
                switch (next)
                {
                    case LenientRecognitionBatchStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientRecognitionBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient recognition batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public LenientRecognitionBatch Copy()
    {
        return new LenientRecognitionBatch(
            Code,
            Label,
            ContactEmail,
            Pinned,
            Unit,
            Team,
            Department,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
