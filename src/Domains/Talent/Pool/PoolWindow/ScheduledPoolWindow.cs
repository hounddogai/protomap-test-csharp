namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pool window is in its life cycle.</summary>
public enum ScheduledPoolWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A scheduled pool window. Its code is unique within a tenant.</summary>
public sealed class ScheduledPoolWindow(string code, string title, string category, double maximum, string currency, double weight, string employeeId, string country, string checksum, ScheduledPoolWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public double Maximum { get; set; } = maximum;
    public string Currency { get; set; } = currency;
    public double Weight { get; set; } = weight;
    public string EmployeeId { get; set; } = employeeId;
    public string Country { get; set; } = country;
    public string Checksum { get; set; } = checksum;
    public ScheduledPoolWindowStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pool window in status requested.</summary>
    public static ScheduledPoolWindow NewScheduledPoolWindow(string code, string title)
    {
        return new ScheduledPoolWindow(
            code,
            title,
            "",
            0.0,
            "",
            0.0,
            "",
            "",
            "",
            ScheduledPoolWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPoolWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPoolWindowStatus next)
    {
        switch (Status)
        {
            case ScheduledPoolWindowStatus.Requested:
                switch (next)
                {
                    case ScheduledPoolWindowStatus.Revoked:
                        return true;
                }
                return false;
            case ScheduledPoolWindowStatus.Granted:
                return next == ScheduledPoolWindowStatus.Revoked || next == ScheduledPoolWindowStatus.Expired;
            case ScheduledPoolWindowStatus.Revoked:
                return next == ScheduledPoolWindowStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ScheduledPoolWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pool window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled pool window"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Maximum + Weight, 2);
    }

    public ScheduledPoolWindow Copy()
    {
        return new ScheduledPoolWindow(
            Code,
            Title,
            Category,
            Maximum,
            Currency,
            Weight,
            EmployeeId,
            Country,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
