namespace Acme.Workforce.Absence.Accrual.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual batch is in its life cycle.</summary>
public enum AccrualBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A accrual batch as the accrual team keeps it.</summary>
public sealed class AccrualBatch(string code, string name, double rating, string ownerId, int revision, string salaryBand, string employeeId, int graceDays, Dictionary<string, string> attributes, string country, AccrualBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public string OwnerId { get; set; } = ownerId;
    public int Revision { get; set; } = revision;
    public string SalaryBand { get; set; } = salaryBand;
    public string EmployeeId { get; set; } = employeeId;
    public int GraceDays { get; set; } = graceDays;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Country { get; set; } = country;
    public AccrualBatchStatus Status { get; set; } = status;

    /// <summary>Starts a accrual batch in status new.</summary>
    public static AccrualBatch NewAccrualBatch(string code, string name)
    {
        return new AccrualBatch(
            code,
            name,
            0.0,
            "",
            0,
            "",
            "",
            0,
            new Dictionary<string, string>(),
            "",
            AccrualBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualBatchStatus next)
    {
        switch (Status)
        {
            case AccrualBatchStatus.New:
                return next == AccrualBatchStatus.Failed;
            case AccrualBatchStatus.Queued:
                switch (next)
                {
                    case AccrualBatchStatus.Failed:
                        return true;
                }
                return false;
            case AccrualBatchStatus.Running:
                switch (next)
                {
                    case AccrualBatchStatus.Done:
                    case AccrualBatchStatus.Failed:
                        return true;
                }
                return false;
            case AccrualBatchStatus.Done:
                switch (next)
                {
                    case AccrualBatchStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"accrual batch"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetExposure()
    {
        return Math.Round((double)GraceDays * (double)Revision, 2);
    }

    public AccrualBatch Copy()
    {
        return new AccrualBatch(
            Code,
            Name,
            Rating,
            OwnerId,
            Revision,
            SalaryBand,
            EmployeeId,
            GraceDays,
            Attributes,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
