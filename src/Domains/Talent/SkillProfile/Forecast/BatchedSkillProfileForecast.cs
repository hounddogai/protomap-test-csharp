namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched skill profile forecast is in its life cycle.</summary>
public enum BatchedSkillProfileForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A batched skill profile forecast. Its code is unique within a tenant.</summary>
public sealed class BatchedSkillProfileForecast(string code, string name, double amount, bool visible, string summary, double discount, string employeeId, string title, BatchedSkillProfileForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public bool Visible { get; set; } = visible;
    public string Summary { get; set; } = summary;
    public double Discount { get; set; } = discount;
    public string EmployeeId { get; set; } = employeeId;
    public string Title { get; set; } = title;
    public BatchedSkillProfileForecastStatus Status { get; set; } = status;

    /// <summary>Starts a batched skill profile forecast in status idle.</summary>
    public static BatchedSkillProfileForecast NewBatchedSkillProfileForecast(string code, string name)
    {
        return new BatchedSkillProfileForecast(
            code,
            name,
            0.0,
            false,
            "",
            0.0,
            "",
            "",
            BatchedSkillProfileForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedSkillProfileForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedSkillProfileForecastStatus next)
    {
        switch (Status)
        {
            case BatchedSkillProfileForecastStatus.Idle:
                switch (next)
                {
                    case BatchedSkillProfileForecastStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedSkillProfileForecastStatus.Warming:
                switch (next)
                {
                    case BatchedSkillProfileForecastStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedSkillProfileForecastStatus.Ready:
                return next == BatchedSkillProfileForecastStatus.Draining || next == BatchedSkillProfileForecastStatus.Stopped;
            case BatchedSkillProfileForecastStatus.Draining:
                return next == BatchedSkillProfileForecastStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BatchedSkillProfileForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched skill profile forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Amount + Discount, 2);
    }

    public BatchedSkillProfileForecast Copy()
    {
        return new BatchedSkillProfileForecast(
            Code,
            Name,
            Amount,
            Visible,
            Summary,
            Discount,
            EmployeeId,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
