namespace Acme.Workforce.Assets.Assignment.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default assignment batch is in its life cycle.</summary>
public enum DefaultAssignmentBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a default assignment batch and its current status.</summary>
public sealed class DefaultAssignmentBatch(string code, string name, int windowDays, int quantity, double threshold, double ratio, double score, string employeeId, string skill, double cost, DefaultAssignmentBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public int Quantity { get; set; } = quantity;
    public double Threshold { get; set; } = threshold;
    public double Ratio { get; set; } = ratio;
    public double Score { get; set; } = score;
    public string EmployeeId { get; set; } = employeeId;
    public string Skill { get; set; } = skill;
    public double Cost { get; set; } = cost;
    public DefaultAssignmentBatchStatus Status { get; set; } = status;

    /// <summary>Starts a default assignment batch in status created.</summary>
    public static DefaultAssignmentBatch NewDefaultAssignmentBatch(string code, string name)
    {
        return new DefaultAssignmentBatch(
            code,
            name,
            0,
            0,
            0.0,
            0.0,
            0.0,
            "",
            "",
            0.0,
            DefaultAssignmentBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAssignmentBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAssignmentBatchStatus next)
    {
        switch (Status)
        {
            case DefaultAssignmentBatchStatus.Created:
                switch (next)
                {
                    case DefaultAssignmentBatchStatus.Validated:
                        return true;
                }
                return false;
            case DefaultAssignmentBatchStatus.Validated:
                return next == DefaultAssignmentBatchStatus.Published || next == DefaultAssignmentBatchStatus.Retired;
            case DefaultAssignmentBatchStatus.Published:
                return next == DefaultAssignmentBatchStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DefaultAssignmentBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default assignment batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedExposure()
    {
        return Math.Round(Threshold * Cost, 2);
    }

    public DefaultAssignmentBatch Copy()
    {
        return new DefaultAssignmentBatch(
            Code,
            Name,
            WindowDays,
            Quantity,
            Threshold,
            Ratio,
            Score,
            EmployeeId,
            Skill,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
