namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due rollup is in its life cycle.</summary>
public enum TrainingDueRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A training due rollup. Its code is unique within a tenant.</summary>
public sealed class TrainingDueRollup(string code, string label, double cost, string jobCode, string ownerId, double margin, string badge, TrainingDueRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public string JobCode { get; set; } = jobCode;
    public string OwnerId { get; set; } = ownerId;
    public double Margin { get; set; } = margin;
    public string Badge { get; set; } = badge;
    public TrainingDueRollupStatus Status { get; set; } = status;

    /// <summary>Starts a training due rollup in status new.</summary>
    public static TrainingDueRollup NewTrainingDueRollup(string code, string label)
    {
        return new TrainingDueRollup(code, label, 0.0, "", "", 0.0, "", TrainingDueRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueRollupStatus next)
    {
        switch (Status)
        {
            case TrainingDueRollupStatus.New:
                return next == TrainingDueRollupStatus.Queued || next == TrainingDueRollupStatus.Done;
            case TrainingDueRollupStatus.Queued:
                switch (next)
                {
                    case TrainingDueRollupStatus.Done:
                    case TrainingDueRollupStatus.Failed:
                        return true;
                }
                return false;
            case TrainingDueRollupStatus.Running:
                switch (next)
                {
                    case TrainingDueRollupStatus.Done:
                        return true;
                }
                return false;
            case TrainingDueRollupStatus.Done:
                switch (next)
                {
                    case TrainingDueRollupStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedTotal()
    {
        return Math.Round(Cost * Margin, 2);
    }

    public TrainingDueRollup Copy()
    {
        return new TrainingDueRollup(Code, Label, Cost, JobCode, OwnerId, Margin, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
