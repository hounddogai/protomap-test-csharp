namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched headcount template is in its life cycle.</summary>
public enum BatchedHeadcountTemplateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One batched headcount template, with what the planners need to act on it.</summary>
public sealed class BatchedHeadcountTemplate(string code, string name, double amount, double margin, bool active, int attempts, string timezone, int quantity, int sequence, string salaryBand, string unit, BatchedHeadcountTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public double Margin { get; set; } = margin;
    public bool Active { get; set; } = active;
    public int Attempts { get; set; } = attempts;
    public string Timezone { get; set; } = timezone;
    public int Quantity { get; set; } = quantity;
    public int Sequence { get; set; } = sequence;
    public string SalaryBand { get; set; } = salaryBand;
    public string Unit { get; set; } = unit;
    public BatchedHeadcountTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a batched headcount template in status new.</summary>
    public static BatchedHeadcountTemplate NewBatchedHeadcountTemplate(string code, string name)
    {
        return new BatchedHeadcountTemplate(
            code,
            name,
            0.0,
            0.0,
            false,
            0,
            "",
            0,
            0,
            "",
            "",
            BatchedHeadcountTemplateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedHeadcountTemplateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedHeadcountTemplateStatus next)
    {
        switch (Status)
        {
            case BatchedHeadcountTemplateStatus.New:
                switch (next)
                {
                    case BatchedHeadcountTemplateStatus.Failed:
                        return true;
                }
                return false;
            case BatchedHeadcountTemplateStatus.Queued:
                return next == BatchedHeadcountTemplateStatus.Done || next == BatchedHeadcountTemplateStatus.Failed;
            case BatchedHeadcountTemplateStatus.Running:
                return next == BatchedHeadcountTemplateStatus.Failed;
            case BatchedHeadcountTemplateStatus.Done:
                switch (next)
                {
                    case BatchedHeadcountTemplateStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedHeadcountTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched headcount template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Quantity - Amount, 2);
    }

    public BatchedHeadcountTemplate Copy()
    {
        return new BatchedHeadcountTemplate(
            Code,
            Name,
            Amount,
            Margin,
            Active,
            Attempts,
            Timezone,
            Quantity,
            Sequence,
            SalaryBand,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
