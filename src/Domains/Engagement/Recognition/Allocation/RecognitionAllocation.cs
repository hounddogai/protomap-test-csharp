namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition allocation is in its life cycle.</summary>
public enum RecognitionAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A recognition allocation. Its code is unique within a tenant.</summary>
public sealed class RecognitionAllocation(string code, string label, double cost, string notes, double hours, int createdAt, RecognitionAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public string Notes { get; set; } = notes;
    public double Hours { get; set; } = hours;
    public int CreatedAt { get; set; } = createdAt;
    public RecognitionAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a recognition allocation in status requested.</summary>
    public static RecognitionAllocation NewRecognitionAllocation(string code, string label)
    {
        return new RecognitionAllocation(code, label, 0.0, "", 0.0, 0, RecognitionAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionAllocationStatus next)
    {
        switch (Status)
        {
            case RecognitionAllocationStatus.Requested:
                switch (next)
                {
                    case RecognitionAllocationStatus.Granted:
                        return true;
                }
                return false;
            case RecognitionAllocationStatus.Granted:
                switch (next)
                {
                    case RecognitionAllocationStatus.Revoked:
                    case RecognitionAllocationStatus.Expired:
                        return true;
                }
                return false;
            case RecognitionAllocationStatus.Revoked:
                return next == RecognitionAllocationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RecognitionAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetValue()
    {
        return Math.Round(Hours + Cost, 2);
    }

    public RecognitionAllocation Copy()
    {
        return new RecognitionAllocation(Code, Label, Cost, Notes, Hours, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
