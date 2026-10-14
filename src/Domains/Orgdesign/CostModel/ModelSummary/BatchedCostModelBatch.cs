namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched cost model batch is in its life cycle.</summary>
public enum BatchedCostModelBatchStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A batched cost model batch. Its code is unique within a tenant.</summary>
public sealed class BatchedCostModelBatch(string code, string title, string phone, double budget, bool visible, int capacity, int endsAt, string department, string summary, BatchedCostModelBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public double Budget { get; set; } = budget;
    public bool Visible { get; set; } = visible;
    public int Capacity { get; set; } = capacity;
    public int EndsAt { get; set; } = endsAt;
    public string Department { get; set; } = department;
    public string Summary { get; set; } = summary;
    public BatchedCostModelBatchStatus Status { get; set; } = status;

    /// <summary>Starts a batched cost model batch in status draft.</summary>
    public static BatchedCostModelBatch NewBatchedCostModelBatch(string code, string title)
    {
        return new BatchedCostModelBatch(
            code,
            title,
            "",
            0.0,
            false,
            0,
            0,
            "",
            "",
            BatchedCostModelBatchStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedCostModelBatchStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedCostModelBatchStatus next)
    {
        switch (Status)
        {
            case BatchedCostModelBatchStatus.Draft:
                switch (next)
                {
                    case BatchedCostModelBatchStatus.Pending:
                    case BatchedCostModelBatchStatus.Closed:
                        return true;
                }
                return false;
            case BatchedCostModelBatchStatus.Pending:
                return next == BatchedCostModelBatchStatus.Active || next == BatchedCostModelBatchStatus.Closed;
            case BatchedCostModelBatchStatus.Active:
                switch (next)
                {
                    case BatchedCostModelBatchStatus.Suspended:
                    case BatchedCostModelBatchStatus.Closed:
                        return true;
                }
                return false;
            case BatchedCostModelBatchStatus.Suspended:
                switch (next)
                {
                    case BatchedCostModelBatchStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedCostModelBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched cost model batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Budget - (double)Capacity, 2);
    }

    public BatchedCostModelBatch Copy()
    {
        return new BatchedCostModelBatch(
            Code,
            Title,
            Phone,
            Budget,
            Visible,
            Capacity,
            EndsAt,
            Department,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
