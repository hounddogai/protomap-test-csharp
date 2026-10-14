namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global retro pay batch is in its life cycle.</summary>
public enum GlobalRetroPayBatchStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A global retro pay batch. Its code is unique within a tenant.</summary>
public sealed class GlobalRetroPayBatch(string code, string name, string externalRef, double amount, string grade, string timezone, double cost, GlobalRetroPayBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public double Amount { get; set; } = amount;
    public string Grade { get; set; } = grade;
    public string Timezone { get; set; } = timezone;
    public double Cost { get; set; } = cost;
    public GlobalRetroPayBatchStatus Status { get; set; } = status;

    /// <summary>Starts a global retro pay batch in status planned.</summary>
    public static GlobalRetroPayBatch NewGlobalRetroPayBatch(string code, string name)
    {
        return new GlobalRetroPayBatch(code, name, "", 0.0, "", "", 0.0, GlobalRetroPayBatchStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalRetroPayBatchStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalRetroPayBatchStatus next)
    {
        switch (Status)
        {
            case GlobalRetroPayBatchStatus.Planned:
                switch (next)
                {
                    case GlobalRetroPayBatchStatus.Scheduled:
                        return true;
                }
                return false;
            case GlobalRetroPayBatchStatus.Scheduled:
                return next == GlobalRetroPayBatchStatus.InProgress;
            case GlobalRetroPayBatchStatus.InProgress:
                return next == GlobalRetroPayBatchStatus.Completed;
            case GlobalRetroPayBatchStatus.Completed:
                switch (next)
                {
                    case GlobalRetroPayBatchStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalRetroPayBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global retro pay batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global retro pay batch"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Amount + Cost, 2);
    }

    public GlobalRetroPayBatch Copy()
    {
        return new GlobalRetroPayBatch(Code, Name, ExternalRef, Amount, Grade, Timezone, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
