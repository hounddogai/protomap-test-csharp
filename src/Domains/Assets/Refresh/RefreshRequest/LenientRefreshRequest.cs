namespace Acme.Workforce.Assets.Refresh.RefreshRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient refresh request is in its life cycle.</summary>
public enum LenientRefreshRequestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient refresh request. Its code is unique within a tenant.</summary>
public sealed class LenientRefreshRequest(string code, string title, int quantity, double hours, string employeeId, string managerId, LenientRefreshRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public double Hours { get; set; } = hours;
    public string EmployeeId { get; set; } = employeeId;
    public string ManagerId { get; set; } = managerId;
    public LenientRefreshRequestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient refresh request in status created.</summary>
    public static LenientRefreshRequest NewLenientRefreshRequest(string code, string title)
    {
        return new LenientRefreshRequest(code, title, 0, 0.0, "", "", LenientRefreshRequestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientRefreshRequestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientRefreshRequestStatus next)
    {
        switch (Status)
        {
            case LenientRefreshRequestStatus.Created:
                switch (next)
                {
                    case LenientRefreshRequestStatus.Published:
                    case LenientRefreshRequestStatus.Retired:
                        return true;
                }
                return false;
            case LenientRefreshRequestStatus.Validated:
                switch (next)
                {
                    case LenientRefreshRequestStatus.Published:
                        return true;
                }
                return false;
            case LenientRefreshRequestStatus.Published:
                return next == LenientRefreshRequestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LenientRefreshRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient refresh request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Quantity * Hours, 2);
    }

    public LenientRefreshRequest Copy()
    {
        return new LenientRefreshRequest(Code, Title, Quantity, Hours, EmployeeId, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
