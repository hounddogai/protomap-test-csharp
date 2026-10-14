namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped pay run record is in its life cycle.</summary>
public enum CappedPayRunRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a capped pay run record and its current status.</summary>
public sealed class CappedPayRunRecord(string code, string name, int retries, string jobCode, double margin, string timezone, double score, string title, int tenureMonths, double hours, CappedPayRunRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string JobCode { get; set; } = jobCode;
    public double Margin { get; set; } = margin;
    public string Timezone { get; set; } = timezone;
    public double Score { get; set; } = score;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Hours { get; set; } = hours;
    public CappedPayRunRecordStatus Status { get; set; } = status;

    /// <summary>Starts a capped pay run record in status draft.</summary>
    public static CappedPayRunRecord NewCappedPayRunRecord(string code, string name)
    {
        return new CappedPayRunRecord(code, name, 0, "", 0.0, "", 0.0, "", 0, 0.0, CappedPayRunRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPayRunRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPayRunRecordStatus next)
    {
        switch (Status)
        {
            case CappedPayRunRecordStatus.Draft:
                switch (next)
                {
                    case CappedPayRunRecordStatus.Pending:
                    case CappedPayRunRecordStatus.Closed:
                        return true;
                }
                return false;
            case CappedPayRunRecordStatus.Pending:
                switch (next)
                {
                    case CappedPayRunRecordStatus.Suspended:
                    case CappedPayRunRecordStatus.Closed:
                        return true;
                }
                return false;
            case CappedPayRunRecordStatus.Active:
                switch (next)
                {
                    case CappedPayRunRecordStatus.Suspended:
                    case CappedPayRunRecordStatus.Closed:
                        return true;
                }
                return false;
            case CappedPayRunRecordStatus.Suspended:
                return next == CappedPayRunRecordStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CappedPayRunRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped pay run record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped pay run record"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Retries - Margin, 2);
    }

    public CappedPayRunRecord Copy()
    {
        return new CappedPayRunRecord(
            Code,
            Name,
            Retries,
            JobCode,
            Margin,
            Timezone,
            Score,
            Title,
            TenureMonths,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
