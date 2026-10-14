namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty mapping is in its life cycle.</summary>
public enum WarrantyMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One warranty mapping, with what the planners need to act on it.</summary>
public sealed class WarrantyMapping(string code, string label, string segment, int batchSize, int attempts, WarrantyMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public int BatchSize { get; set; } = batchSize;
    public int Attempts { get; set; } = attempts;
    public WarrantyMappingStatus Status { get; set; } = status;

    /// <summary>Starts a warranty mapping in status planned.</summary>
    public static WarrantyMapping NewWarrantyMapping(string code, string label)
    {
        return new WarrantyMapping(code, label, "", 0, 0, WarrantyMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyMappingStatus next)
    {
        switch (Status)
        {
            case WarrantyMappingStatus.Planned:
                return next == WarrantyMappingStatus.InProgress || next == WarrantyMappingStatus.Completed;
            case WarrantyMappingStatus.Scheduled:
                return next == WarrantyMappingStatus.InProgress;
            case WarrantyMappingStatus.InProgress:
                switch (next)
                {
                    case WarrantyMappingStatus.Completed:
                    case WarrantyMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case WarrantyMappingStatus.Completed:
                return next == WarrantyMappingStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(WarrantyMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)BatchSize * (double)Attempts, 2);
    }

    public WarrantyMapping Copy()
    {
        return new WarrantyMapping(Code, Label, Segment, BatchSize, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
