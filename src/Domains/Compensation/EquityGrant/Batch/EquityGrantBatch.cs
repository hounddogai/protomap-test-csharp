namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant batch is in its life cycle.</summary>
public enum EquityGrantBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One equity grant batch, with what the planners need to act on it.</summary>
public sealed class EquityGrantBatch(string code, string title, string employeeId, double weight, bool visible, string skill, double hours, double margin, string badge, double minimum, int batchSize, EquityGrantBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public double Weight { get; set; } = weight;
    public bool Visible { get; set; } = visible;
    public string Skill { get; set; } = skill;
    public double Hours { get; set; } = hours;
    public double Margin { get; set; } = margin;
    public string Badge { get; set; } = badge;
    public double Minimum { get; set; } = minimum;
    public int BatchSize { get; set; } = batchSize;
    public EquityGrantBatchStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant batch in status new.</summary>
    public static EquityGrantBatch NewEquityGrantBatch(string code, string title)
    {
        return new EquityGrantBatch(
            code,
            title,
            "",
            0.0,
            false,
            "",
            0.0,
            0.0,
            "",
            0.0,
            0,
            EquityGrantBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantBatchStatus next)
    {
        switch (Status)
        {
            case EquityGrantBatchStatus.New:
                return next == EquityGrantBatchStatus.Done;
            case EquityGrantBatchStatus.Queued:
                switch (next)
                {
                    case EquityGrantBatchStatus.Running:
                        return true;
                }
                return false;
            case EquityGrantBatchStatus.Running:
                return next == EquityGrantBatchStatus.Done || next == EquityGrantBatchStatus.Failed;
            case EquityGrantBatchStatus.Done:
                return next == EquityGrantBatchStatus.Failed;
        }
        return false;
    }

    public void MoveTo(EquityGrantBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Margin + (double)BatchSize, 2);
    }

    public EquityGrantBatch Copy()
    {
        return new EquityGrantBatch(
            Code,
            Title,
            EmployeeId,
            Weight,
            Visible,
            Skill,
            Hours,
            Margin,
            Badge,
            Minimum,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
