namespace Acme.Workforce.Payrollops.TaxForm.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched tax form segment is in its life cycle.</summary>
public enum BatchedTaxFormSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One batched tax form segment, with what the planners need to act on it.</summary>
public sealed class BatchedTaxFormSegment(string code, string label, string costCenter, string skill, bool pinned, Dictionary<string, string> attributes, int startsAt, BatchedTaxFormSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public string Skill { get; set; } = skill;
    public bool Pinned { get; set; } = pinned;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int StartsAt { get; set; } = startsAt;
    public BatchedTaxFormSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a batched tax form segment in status requested.</summary>
    public static BatchedTaxFormSegment NewBatchedTaxFormSegment(string code, string label)
    {
        return new BatchedTaxFormSegment(
            code,
            label,
            "",
            "",
            false,
            new Dictionary<string, string>(),
            0,
            BatchedTaxFormSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTaxFormSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTaxFormSegmentStatus next)
    {
        switch (Status)
        {
            case BatchedTaxFormSegmentStatus.Requested:
                return next == BatchedTaxFormSegmentStatus.Granted || next == BatchedTaxFormSegmentStatus.Expired;
            case BatchedTaxFormSegmentStatus.Granted:
                switch (next)
                {
                    case BatchedTaxFormSegmentStatus.Revoked:
                    case BatchedTaxFormSegmentStatus.Expired:
                        return true;
                }
                return false;
            case BatchedTaxFormSegmentStatus.Revoked:
                switch (next)
                {
                    case BatchedTaxFormSegmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedTaxFormSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched tax form segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public BatchedTaxFormSegment Copy()
    {
        return new BatchedTaxFormSegment(Code, Label, CostCenter, Skill, Pinned, Attributes, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
