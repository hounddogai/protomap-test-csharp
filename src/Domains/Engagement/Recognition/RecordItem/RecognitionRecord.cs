namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition record is in its life cycle.</summary>
public enum RecognitionRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One recognition record, with what the planners need to act on it.</summary>
public sealed class RecognitionRecord(string code, string label, string externalRef, string costCenter, string name, double amount, RecognitionRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string CostCenter { get; set; } = costCenter;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public RecognitionRecordStatus Status { get; set; } = status;

    /// <summary>Starts a recognition record in status created.</summary>
    public static RecognitionRecord NewRecognitionRecord(string code, string label)
    {
        return new RecognitionRecord(code, label, "", "", "", 0.0, RecognitionRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionRecordStatus next)
    {
        switch (Status)
        {
            case RecognitionRecordStatus.Created:
                switch (next)
                {
                    case RecognitionRecordStatus.Retired:
                        return true;
                }
                return false;
            case RecognitionRecordStatus.Validated:
                return next == RecognitionRecordStatus.Published || next == RecognitionRecordStatus.Retired;
            case RecognitionRecordStatus.Published:
                return next == RecognitionRecordStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RecognitionRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public RecognitionRecord Copy()
    {
        return new RecognitionRecord(Code, Label, ExternalRef, CostCenter, Name, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
