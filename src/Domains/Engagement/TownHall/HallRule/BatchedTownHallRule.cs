namespace Acme.Workforce.Engagement.TownHall.HallRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched town hall rule is in its life cycle.</summary>
public enum BatchedTownHallRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A batched town hall rule. Its code is unique within a tenant.</summary>
public sealed class BatchedTownHallRule(string code, string name, string title, int retries, string unit, BatchedTownHallRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string Unit { get; set; } = unit;
    public BatchedTownHallRuleStatus Status { get; set; } = status;

    /// <summary>Starts a batched town hall rule in status idle.</summary>
    public static BatchedTownHallRule NewBatchedTownHallRule(string code, string name)
    {
        return new BatchedTownHallRule(code, name, "", 0, "", BatchedTownHallRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTownHallRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTownHallRuleStatus next)
    {
        switch (Status)
        {
            case BatchedTownHallRuleStatus.Idle:
                return next == BatchedTownHallRuleStatus.Draining || next == BatchedTownHallRuleStatus.Stopped;
            case BatchedTownHallRuleStatus.Warming:
                switch (next)
                {
                    case BatchedTownHallRuleStatus.Draining:
                    case BatchedTownHallRuleStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedTownHallRuleStatus.Ready:
                switch (next)
                {
                    case BatchedTownHallRuleStatus.Draining:
                    case BatchedTownHallRuleStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedTownHallRuleStatus.Draining:
                switch (next)
                {
                    case BatchedTownHallRuleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedTownHallRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched town hall rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public BatchedTownHallRule Copy()
    {
        return new BatchedTownHallRule(Code, Name, Title, Retries, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
