namespace Acme.Workforce.Assets.Laptop.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched laptop template is in its life cycle.</summary>
public enum BatchedLaptopTemplateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A batched laptop template. Its code is unique within a tenant.</summary>
public sealed class BatchedLaptopTemplate(string code, string title, string source, int capacity, int priority, BatchedLaptopTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public int Capacity { get; set; } = capacity;
    public int Priority { get; set; } = priority;
    public BatchedLaptopTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a batched laptop template in status draft.</summary>
    public static BatchedLaptopTemplate NewBatchedLaptopTemplate(string code, string title)
    {
        return new BatchedLaptopTemplate(code, title, "", 0, 0, BatchedLaptopTemplateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedLaptopTemplateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedLaptopTemplateStatus next)
    {
        switch (Status)
        {
            case BatchedLaptopTemplateStatus.Draft:
                switch (next)
                {
                    case BatchedLaptopTemplateStatus.Pending:
                        return true;
                }
                return false;
            case BatchedLaptopTemplateStatus.Pending:
                switch (next)
                {
                    case BatchedLaptopTemplateStatus.Active:
                    case BatchedLaptopTemplateStatus.Suspended:
                        return true;
                }
                return false;
            case BatchedLaptopTemplateStatus.Active:
                return next == BatchedLaptopTemplateStatus.Suspended || next == BatchedLaptopTemplateStatus.Closed;
            case BatchedLaptopTemplateStatus.Suspended:
                switch (next)
                {
                    case BatchedLaptopTemplateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedLaptopTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched laptop template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Priority - (double)Capacity, 2);
    }

    public BatchedLaptopTemplate Copy()
    {
        return new BatchedLaptopTemplate(Code, Title, Source, Capacity, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
