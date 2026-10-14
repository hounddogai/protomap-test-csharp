namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped cost model checkpoint is in its life cycle.</summary>
public enum CappedCostModelCheckpointStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A capped cost model checkpoint as the cost model team keeps it.</summary>
public sealed class CappedCostModelCheckpoint(string code, string label, int quantity, int createdAt, double hours, double threshold, string timezone, int attempts, int updatedAt, bool visible, CappedCostModelCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public int CreatedAt { get; set; } = createdAt;
    public double Hours { get; set; } = hours;
    public double Threshold { get; set; } = threshold;
    public string Timezone { get; set; } = timezone;
    public int Attempts { get; set; } = attempts;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Visible { get; set; } = visible;
    public CappedCostModelCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a capped cost model checkpoint in status requested.</summary>
    public static CappedCostModelCheckpoint NewCappedCostModelCheckpoint(string code, string label)
    {
        return new CappedCostModelCheckpoint(
            code,
            label,
            0,
            0,
            0.0,
            0.0,
            "",
            0,
            0,
            false,
            CappedCostModelCheckpointStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedCostModelCheckpointStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedCostModelCheckpointStatus next)
    {
        switch (Status)
        {
            case CappedCostModelCheckpointStatus.Requested:
                return next == CappedCostModelCheckpointStatus.Revoked;
            case CappedCostModelCheckpointStatus.Granted:
                switch (next)
                {
                    case CappedCostModelCheckpointStatus.Revoked:
                    case CappedCostModelCheckpointStatus.Expired:
                        return true;
                }
                return false;
            case CappedCostModelCheckpointStatus.Revoked:
                switch (next)
                {
                    case CappedCostModelCheckpointStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedCostModelCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped cost model checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"capped cost model checkpoint"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Quantity * (double)UpdatedAt, 2);
    }

    public CappedCostModelCheckpoint Copy()
    {
        return new CappedCostModelCheckpoint(
            Code,
            Label,
            Quantity,
            CreatedAt,
            Hours,
            Threshold,
            Timezone,
            Attempts,
            UpdatedAt,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
