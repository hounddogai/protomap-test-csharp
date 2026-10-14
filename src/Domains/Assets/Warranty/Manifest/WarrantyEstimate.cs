namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty estimate is in its life cycle.</summary>
public enum WarrantyEstimateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One warranty estimate, with what the planners need to act on it.</summary>
public sealed class WarrantyEstimate(string code, string label, string region, int leaveDays, int sequence, int capacity, int retries, WarrantyEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public int LeaveDays { get; set; } = leaveDays;
    public int Sequence { get; set; } = sequence;
    public int Capacity { get; set; } = capacity;
    public int Retries { get; set; } = retries;
    public WarrantyEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a warranty estimate in status created.</summary>
    public static WarrantyEstimate NewWarrantyEstimate(string code, string label)
    {
        return new WarrantyEstimate(code, label, "", 0, 0, 0, 0, WarrantyEstimateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyEstimateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyEstimateStatus next)
    {
        switch (Status)
        {
            case WarrantyEstimateStatus.Created:
                switch (next)
                {
                    case WarrantyEstimateStatus.Published:
                        return true;
                }
                return false;
            case WarrantyEstimateStatus.Validated:
                return next == WarrantyEstimateStatus.Retired;
            case WarrantyEstimateStatus.Published:
                switch (next)
                {
                    case WarrantyEstimateStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"warranty estimate"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Capacity + (double)Retries, 2);
    }

    public WarrantyEstimate Copy()
    {
        return new WarrantyEstimate(Code, Label, Region, LeaveDays, Sequence, Capacity, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
