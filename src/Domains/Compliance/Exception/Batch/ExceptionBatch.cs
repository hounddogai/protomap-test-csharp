namespace Acme.Workforce.Compliance.Exception.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception batch is in its life cycle.</summary>
public enum ExceptionBatchStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A exception batch. Its code is unique within a tenant.</summary>
public sealed class ExceptionBatch(string code, string title, double rate, string currency, string team, bool approved, string jobCode, double minimum, double score, int sequence, string summary, ExceptionBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public string Currency { get; set; } = currency;
    public string Team { get; set; } = team;
    public bool Approved { get; set; } = approved;
    public string JobCode { get; set; } = jobCode;
    public double Minimum { get; set; } = minimum;
    public double Score { get; set; } = score;
    public int Sequence { get; set; } = sequence;
    public string Summary { get; set; } = summary;
    public ExceptionBatchStatus Status { get; set; } = status;

    /// <summary>Starts a exception batch in status draft.</summary>
    public static ExceptionBatch NewExceptionBatch(string code, string title)
    {
        return new ExceptionBatch(code, title, 0.0, "", "", false, "", 0.0, 0.0, 0, "", ExceptionBatchStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionBatchStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionBatchStatus next)
    {
        switch (Status)
        {
            case ExceptionBatchStatus.Draft:
                return next == ExceptionBatchStatus.Active;
            case ExceptionBatchStatus.Pending:
                switch (next)
                {
                    case ExceptionBatchStatus.Closed:
                        return true;
                }
                return false;
            case ExceptionBatchStatus.Active:
                switch (next)
                {
                    case ExceptionBatchStatus.Suspended:
                    case ExceptionBatchStatus.Closed:
                        return true;
                }
                return false;
            case ExceptionBatchStatus.Suspended:
                switch (next)
                {
                    case ExceptionBatchStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"exception batch"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Minimum - Rate, 2);
    }

    public ExceptionBatch Copy()
    {
        return new ExceptionBatch(
            Code,
            Title,
            Rate,
            Currency,
            Team,
            Approved,
            JobCode,
            Minimum,
            Score,
            Sequence,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
