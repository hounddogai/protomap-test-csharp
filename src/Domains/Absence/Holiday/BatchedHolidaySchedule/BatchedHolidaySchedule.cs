namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched holiday schedule is in its life cycle.</summary>
public enum BatchedHolidayScheduleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A batched holiday schedule. Its code is unique within a tenant.</summary>
public sealed class BatchedHolidaySchedule(string code, string name, string jobCode, string description, double cost, string managerId, BatchedHolidayScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string Description { get; set; } = description;
    public double Cost { get; set; } = cost;
    public string ManagerId { get; set; } = managerId;
    public BatchedHolidayScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a batched holiday schedule in status created.</summary>
    public static BatchedHolidaySchedule NewBatchedHolidaySchedule(string code, string name)
    {
        return new BatchedHolidaySchedule(code, name, "", "", 0.0, "", BatchedHolidayScheduleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedHolidayScheduleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedHolidayScheduleStatus next)
    {
        switch (Status)
        {
            case BatchedHolidayScheduleStatus.Created:
                return next == BatchedHolidayScheduleStatus.Published;
            case BatchedHolidayScheduleStatus.Validated:
                switch (next)
                {
                    case BatchedHolidayScheduleStatus.Published:
                        return true;
                }
                return false;
            case BatchedHolidayScheduleStatus.Published:
                return next == BatchedHolidayScheduleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BatchedHolidayScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched holiday schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public BatchedHolidaySchedule Copy()
    {
        return new BatchedHolidaySchedule(Code, Name, JobCode, Description, Cost, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
