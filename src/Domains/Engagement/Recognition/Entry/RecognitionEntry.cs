namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition entry is in its life cycle.</summary>
public enum RecognitionEntryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One recognition entry, with what the planners need to act on it.</summary>
public sealed class RecognitionEntry(string code, string label, double minimum, double floorValue, bool visible, string costCenter, string reviewer, string category, double amount, double threshold, double margin, RecognitionEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public double FloorValue { get; set; } = floorValue;
    public bool Visible { get; set; } = visible;
    public string CostCenter { get; set; } = costCenter;
    public string Reviewer { get; set; } = reviewer;
    public string Category { get; set; } = category;
    public double Amount { get; set; } = amount;
    public double Threshold { get; set; } = threshold;
    public double Margin { get; set; } = margin;
    public RecognitionEntryStatus Status { get; set; } = status;

    /// <summary>Starts a recognition entry in status requested.</summary>
    public static RecognitionEntry NewRecognitionEntry(string code, string label)
    {
        return new RecognitionEntry(
            code,
            label,
            0.0,
            0.0,
            false,
            "",
            "",
            "",
            0.0,
            0.0,
            0.0,
            RecognitionEntryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionEntryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionEntryStatus next)
    {
        switch (Status)
        {
            case RecognitionEntryStatus.Requested:
                return next == RecognitionEntryStatus.Expired;
            case RecognitionEntryStatus.Granted:
                switch (next)
                {
                    case RecognitionEntryStatus.Revoked:
                        return true;
                }
                return false;
            case RecognitionEntryStatus.Revoked:
                switch (next)
                {
                    case RecognitionEntryStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Amount + Threshold, 2);
    }

    public RecognitionEntry Copy()
    {
        return new RecognitionEntry(
            Code,
            Label,
            Minimum,
            FloorValue,
            Visible,
            CostCenter,
            Reviewer,
            Category,
            Amount,
            Threshold,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
