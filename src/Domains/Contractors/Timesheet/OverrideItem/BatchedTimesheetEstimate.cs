namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched timesheet estimate is in its life cycle.</summary>
public enum BatchedTimesheetEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One batched timesheet estimate, with what the planners need to act on it.</summary>
public sealed class BatchedTimesheetEstimate(string code, string title, double rating, string notes, string managerId, string contactEmail, BatchedTimesheetEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string Notes { get; set; } = notes;
    public string ManagerId { get; set; } = managerId;
    public string ContactEmail { get; set; } = contactEmail;
    public BatchedTimesheetEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a batched timesheet estimate in status requested.</summary>
    public static BatchedTimesheetEstimate NewBatchedTimesheetEstimate(string code, string title)
    {
        return new BatchedTimesheetEstimate(code, title, 0.0, "", "", "", BatchedTimesheetEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTimesheetEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTimesheetEstimateStatus next)
    {
        switch (Status)
        {
            case BatchedTimesheetEstimateStatus.Requested:
                return next == BatchedTimesheetEstimateStatus.Expired;
            case BatchedTimesheetEstimateStatus.Granted:
                return next == BatchedTimesheetEstimateStatus.Expired;
            case BatchedTimesheetEstimateStatus.Revoked:
                switch (next)
                {
                    case BatchedTimesheetEstimateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedTimesheetEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched timesheet estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Rating;
    }

    public BatchedTimesheetEstimate Copy()
    {
        return new BatchedTimesheetEstimate(Code, Title, Rating, Notes, ManagerId, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
