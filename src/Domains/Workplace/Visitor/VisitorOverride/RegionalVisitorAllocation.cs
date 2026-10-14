namespace Acme.Workforce.Workplace.Visitor.VisitorOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional visitor allocation is in its life cycle.</summary>
public enum RegionalVisitorAllocationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A regional visitor allocation. Its code is unique within a tenant.</summary>
public sealed class RegionalVisitorAllocation(string code, string label, string description, int retries, int leaveDays, string phone, RegionalVisitorAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public int Retries { get; set; } = retries;
    public int LeaveDays { get; set; } = leaveDays;
    public string Phone { get; set; } = phone;
    public RegionalVisitorAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a regional visitor allocation in status open.</summary>
    public static RegionalVisitorAllocation NewRegionalVisitorAllocation(string code, string label)
    {
        return new RegionalVisitorAllocation(code, label, "", 0, 0, "", RegionalVisitorAllocationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalVisitorAllocationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalVisitorAllocationStatus next)
    {
        switch (Status)
        {
            case RegionalVisitorAllocationStatus.Open:
                switch (next)
                {
                    case RegionalVisitorAllocationStatus.InReview:
                    case RegionalVisitorAllocationStatus.Archived:
                        return true;
                }
                return false;
            case RegionalVisitorAllocationStatus.InReview:
                switch (next)
                {
                    case RegionalVisitorAllocationStatus.Reopened:
                        return true;
                }
                return false;
            case RegionalVisitorAllocationStatus.Resolved:
                switch (next)
                {
                    case RegionalVisitorAllocationStatus.Reopened:
                        return true;
                }
                return false;
            case RegionalVisitorAllocationStatus.Reopened:
                switch (next)
                {
                    case RegionalVisitorAllocationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalVisitorAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional visitor allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"regional visitor allocation"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Retries * (double)LeaveDays, 2);
    }

    public RegionalVisitorAllocation Copy()
    {
        return new RegionalVisitorAllocation(Code, Label, Description, Retries, LeaveDays, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
