namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched retro pay forecast is in its life cycle.</summary>
public enum BatchedRetroPayForecastStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A batched retro pay forecast. Its code is unique within a tenant.</summary>
public sealed class BatchedRetroPayForecast(string code, string title, string employeeId, string locale, string grade, double minimum, int capacity, string externalRef, int endsAt, BatchedRetroPayForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public string Locale { get; set; } = locale;
    public string Grade { get; set; } = grade;
    public double Minimum { get; set; } = minimum;
    public int Capacity { get; set; } = capacity;
    public string ExternalRef { get; set; } = externalRef;
    public int EndsAt { get; set; } = endsAt;
    public BatchedRetroPayForecastStatus Status { get; set; } = status;

    /// <summary>Starts a batched retro pay forecast in status planned.</summary>
    public static BatchedRetroPayForecast NewBatchedRetroPayForecast(string code, string title)
    {
        return new BatchedRetroPayForecast(
            code,
            title,
            "",
            "",
            "",
            0.0,
            0,
            "",
            0,
            BatchedRetroPayForecastStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedRetroPayForecastStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedRetroPayForecastStatus next)
    {
        switch (Status)
        {
            case BatchedRetroPayForecastStatus.Planned:
                return next == BatchedRetroPayForecastStatus.Scheduled || next == BatchedRetroPayForecastStatus.Cancelled;
            case BatchedRetroPayForecastStatus.Scheduled:
                switch (next)
                {
                    case BatchedRetroPayForecastStatus.InProgress:
                    case BatchedRetroPayForecastStatus.Completed:
                        return true;
                }
                return false;
            case BatchedRetroPayForecastStatus.InProgress:
                switch (next)
                {
                    case BatchedRetroPayForecastStatus.Completed:
                    case BatchedRetroPayForecastStatus.Cancelled:
                        return true;
                }
                return false;
            case BatchedRetroPayForecastStatus.Completed:
                switch (next)
                {
                    case BatchedRetroPayForecastStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedRetroPayForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched retro pay forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Minimum - (double)EndsAt, 2);
    }

    public BatchedRetroPayForecast Copy()
    {
        return new BatchedRetroPayForecast(
            Code,
            Title,
            EmployeeId,
            Locale,
            Grade,
            Minimum,
            Capacity,
            ExternalRef,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
