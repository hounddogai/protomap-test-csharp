namespace Acme.Workforce.Payrollops.Garnishment.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment record is in its life cycle.</summary>
public enum GarnishmentRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A garnishment record as the garnishment team keeps it.</summary>
public sealed class GarnishmentRecord(string code, string name, string category, double ceiling, int batchSize, int expiresAt, string salaryBand, string grade, GarnishmentRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public double Ceiling { get; set; } = ceiling;
    public int BatchSize { get; set; } = batchSize;
    public int ExpiresAt { get; set; } = expiresAt;
    public string SalaryBand { get; set; } = salaryBand;
    public string Grade { get; set; } = grade;
    public GarnishmentRecordStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment record in status planned.</summary>
    public static GarnishmentRecord NewGarnishmentRecord(string code, string name)
    {
        return new GarnishmentRecord(code, name, "", 0.0, 0, 0, "", "", GarnishmentRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentRecordStatus next)
    {
        switch (Status)
        {
            case GarnishmentRecordStatus.Planned:
                switch (next)
                {
                    case GarnishmentRecordStatus.Completed:
                        return true;
                }
                return false;
            case GarnishmentRecordStatus.Scheduled:
                return next == GarnishmentRecordStatus.InProgress || next == GarnishmentRecordStatus.Cancelled;
            case GarnishmentRecordStatus.InProgress:
                return next == GarnishmentRecordStatus.Completed || next == GarnishmentRecordStatus.Cancelled;
            case GarnishmentRecordStatus.Completed:
                return next == GarnishmentRecordStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(GarnishmentRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"garnishment record"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Ceiling - (double)ExpiresAt, 2);
    }

    public GarnishmentRecord Copy()
    {
        return new GarnishmentRecord(Code, Name, Category, Ceiling, BatchSize, ExpiresAt, SalaryBand, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
