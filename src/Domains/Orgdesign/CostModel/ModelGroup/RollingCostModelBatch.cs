namespace Acme.Workforce.Orgdesign.CostModel.ModelGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling cost model batch is in its life cycle.</summary>
public enum RollingCostModelBatchStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One rolling cost model batch, with what the planners need to act on it.</summary>
public sealed class RollingCostModelBatch(string code, string label, string skill, string team, string currency, int createdAt, RollingCostModelBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public string Team { get; set; } = team;
    public string Currency { get; set; } = currency;
    public int CreatedAt { get; set; } = createdAt;
    public RollingCostModelBatchStatus Status { get; set; } = status;

    /// <summary>Starts a rolling cost model batch in status idle.</summary>
    public static RollingCostModelBatch NewRollingCostModelBatch(string code, string label)
    {
        return new RollingCostModelBatch(code, label, "", "", "", 0, RollingCostModelBatchStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingCostModelBatchStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingCostModelBatchStatus next)
    {
        switch (Status)
        {
            case RollingCostModelBatchStatus.Idle:
                switch (next)
                {
                    case RollingCostModelBatchStatus.Stopped:
                        return true;
                }
                return false;
            case RollingCostModelBatchStatus.Warming:
                switch (next)
                {
                    case RollingCostModelBatchStatus.Stopped:
                        return true;
                }
                return false;
            case RollingCostModelBatchStatus.Ready:
                switch (next)
                {
                    case RollingCostModelBatchStatus.Stopped:
                        return true;
                }
                return false;
            case RollingCostModelBatchStatus.Draining:
                switch (next)
                {
                    case RollingCostModelBatchStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingCostModelBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling cost model batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"rolling cost model batch"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public RollingCostModelBatch Copy()
    {
        return new RollingCostModelBatch(Code, Label, Skill, Team, Currency, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
