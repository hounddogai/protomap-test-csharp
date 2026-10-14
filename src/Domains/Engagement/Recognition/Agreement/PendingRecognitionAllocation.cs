namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending recognition allocation is in its life cycle.</summary>
public enum PendingRecognitionAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending recognition allocation. Its code is unique within a tenant.</summary>
public sealed class PendingRecognitionAllocation(string code, string label, string checksum, int capacity, int startsAt, int graceDays, int tenureMonths, string costCenter, int durationMinutes, Dictionary<string, string> attributes, PendingRecognitionAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public int Capacity { get; set; } = capacity;
    public int StartsAt { get; set; } = startsAt;
    public int GraceDays { get; set; } = graceDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public string CostCenter { get; set; } = costCenter;
    public int DurationMinutes { get; set; } = durationMinutes;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public PendingRecognitionAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a pending recognition allocation in status requested.</summary>
    public static PendingRecognitionAllocation NewPendingRecognitionAllocation(string code, string label)
    {
        return new PendingRecognitionAllocation(
            code,
            label,
            "",
            0,
            0,
            0,
            0,
            "",
            0,
            new Dictionary<string, string>(),
            PendingRecognitionAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingRecognitionAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingRecognitionAllocationStatus next)
    {
        switch (Status)
        {
            case PendingRecognitionAllocationStatus.Requested:
                return next == PendingRecognitionAllocationStatus.Granted || next == PendingRecognitionAllocationStatus.Expired;
            case PendingRecognitionAllocationStatus.Granted:
                return next == PendingRecognitionAllocationStatus.Expired;
            case PendingRecognitionAllocationStatus.Revoked:
                return next == PendingRecognitionAllocationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingRecognitionAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending recognition allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedExposure()
    {
        return Math.Round((double)DurationMinutes * (double)Capacity, 2);
    }

    public PendingRecognitionAllocation Copy()
    {
        return new PendingRecognitionAllocation(
            Code,
            Label,
            Checksum,
            Capacity,
            StartsAt,
            GraceDays,
            TenureMonths,
            CostCenter,
            DurationMinutes,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
