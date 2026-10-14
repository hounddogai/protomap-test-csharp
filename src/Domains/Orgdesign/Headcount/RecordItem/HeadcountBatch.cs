namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount batch is in its life cycle.</summary>
public enum HeadcountBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One headcount batch, with what the planners need to act on it.</summary>
public sealed class HeadcountBatch(string code, string name, int retries, bool archived, int batchSize, int createdAt, string department, int startsAt, double discount, double amount, string contactEmail, HeadcountBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public bool Archived { get; set; } = archived;
    public int BatchSize { get; set; } = batchSize;
    public int CreatedAt { get; set; } = createdAt;
    public string Department { get; set; } = department;
    public int StartsAt { get; set; } = startsAt;
    public double Discount { get; set; } = discount;
    public double Amount { get; set; } = amount;
    public string ContactEmail { get; set; } = contactEmail;
    public HeadcountBatchStatus Status { get; set; } = status;

    /// <summary>Starts a headcount batch in status open.</summary>
    public static HeadcountBatch NewHeadcountBatch(string code, string name)
    {
        return new HeadcountBatch(code, name, 0, false, 0, 0, "", 0, 0.0, 0.0, "", HeadcountBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountBatchStatus next)
    {
        switch (Status)
        {
            case HeadcountBatchStatus.Open:
                return next == HeadcountBatchStatus.Archived;
            case HeadcountBatchStatus.InReview:
                return next == HeadcountBatchStatus.Reopened;
            case HeadcountBatchStatus.Resolved:
                return next == HeadcountBatchStatus.Reopened || next == HeadcountBatchStatus.Archived;
            case HeadcountBatchStatus.Reopened:
                switch (next)
                {
                    case HeadcountBatchStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetLoad()
    {
        return Math.Round((double)BatchSize + (double)Retries, 2);
    }

    public HeadcountBatch Copy()
    {
        return new HeadcountBatch(
            Code,
            Name,
            Retries,
            Archived,
            BatchSize,
            CreatedAt,
            Department,
            StartsAt,
            Discount,
            Amount,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
