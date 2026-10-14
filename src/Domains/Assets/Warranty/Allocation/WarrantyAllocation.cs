namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty allocation is in its life cycle.</summary>
public enum WarrantyAllocationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One warranty allocation, with what the planners need to act on it.</summary>
public sealed class WarrantyAllocation(string code, string name, string notes, double rating, double ratio, int tenureMonths, string segment, WarrantyAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public double Rating { get; set; } = rating;
    public double Ratio { get; set; } = ratio;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Segment { get; set; } = segment;
    public WarrantyAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a warranty allocation in status open.</summary>
    public static WarrantyAllocation NewWarrantyAllocation(string code, string name)
    {
        return new WarrantyAllocation(code, name, "", 0.0, 0.0, 0, "", WarrantyAllocationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyAllocationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyAllocationStatus next)
    {
        switch (Status)
        {
            case WarrantyAllocationStatus.Open:
                return next == WarrantyAllocationStatus.InReview;
            case WarrantyAllocationStatus.InReview:
                return next == WarrantyAllocationStatus.Reopened;
            case WarrantyAllocationStatus.Resolved:
                switch (next)
                {
                    case WarrantyAllocationStatus.Reopened:
                    case WarrantyAllocationStatus.Archived:
                        return true;
                }
                return false;
            case WarrantyAllocationStatus.Reopened:
                switch (next)
                {
                    case WarrantyAllocationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Rating - (double)TenureMonths, 2);
    }

    public WarrantyAllocation Copy()
    {
        return new WarrantyAllocation(Code, Name, Notes, Rating, Ratio, TenureMonths, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
