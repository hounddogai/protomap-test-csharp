namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional leave request item is in its life cycle.</summary>
public enum RegionalLeaveRequestItemStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A regional leave request item. Its code is unique within a tenant.</summary>
public sealed class RegionalLeaveRequestItem(string code, string label, int leaveDays, string location, int batchSize, double discount, string summary, int sequence, string jobCode, string contactEmail, int priority, RegionalLeaveRequestItemStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public string Location { get; set; } = location;
    public int BatchSize { get; set; } = batchSize;
    public double Discount { get; set; } = discount;
    public string Summary { get; set; } = summary;
    public int Sequence { get; set; } = sequence;
    public string JobCode { get; set; } = jobCode;
    public string ContactEmail { get; set; } = contactEmail;
    public int Priority { get; set; } = priority;
    public RegionalLeaveRequestItemStatus Status { get; set; } = status;

    /// <summary>Starts a regional leave request item in status created.</summary>
    public static RegionalLeaveRequestItem NewRegionalLeaveRequestItem(string code, string label)
    {
        return new RegionalLeaveRequestItem(
            code,
            label,
            0,
            "",
            0,
            0.0,
            "",
            0,
            "",
            "",
            0,
            RegionalLeaveRequestItemStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalLeaveRequestItemStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalLeaveRequestItemStatus next)
    {
        switch (Status)
        {
            case RegionalLeaveRequestItemStatus.Created:
                return next == RegionalLeaveRequestItemStatus.Published || next == RegionalLeaveRequestItemStatus.Retired;
            case RegionalLeaveRequestItemStatus.Validated:
                switch (next)
                {
                    case RegionalLeaveRequestItemStatus.Retired:
                        return true;
                }
                return false;
            case RegionalLeaveRequestItemStatus.Published:
                switch (next)
                {
                    case RegionalLeaveRequestItemStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalLeaveRequestItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional leave request item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"regional leave request item"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Sequence - (double)BatchSize, 2);
    }

    public RegionalLeaveRequestItem Copy()
    {
        return new RegionalLeaveRequestItem(
            Code,
            Label,
            LeaveDays,
            Location,
            BatchSize,
            Discount,
            Summary,
            Sequence,
            JobCode,
            ContactEmail,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
