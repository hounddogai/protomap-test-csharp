namespace Acme.Workforce.Payrollops.PayRun.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pay run budget is in its life cycle.</summary>
public enum ScheduledPayRunBudgetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A scheduled pay run budget. Its code is unique within a tenant.</summary>
public sealed class ScheduledPayRunBudget(string code, string name, bool visible, double rate, string team, int quantity, List<string> tags, int batchSize, int endsAt, string category, int version, ScheduledPayRunBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public double Rate { get; set; } = rate;
    public string Team { get; set; } = team;
    public int Quantity { get; set; } = quantity;
    public List<string> Tags { get; set; } = tags;
    public int BatchSize { get; set; } = batchSize;
    public int EndsAt { get; set; } = endsAt;
    public string Category { get; set; } = category;
    public int Version { get; set; } = version;
    public ScheduledPayRunBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pay run budget in status new.</summary>
    public static ScheduledPayRunBudget NewScheduledPayRunBudget(string code, string name)
    {
        return new ScheduledPayRunBudget(
            code,
            name,
            false,
            0.0,
            "",
            0,
            new List<string>(),
            0,
            0,
            "",
            0,
            ScheduledPayRunBudgetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPayRunBudgetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPayRunBudgetStatus next)
    {
        switch (Status)
        {
            case ScheduledPayRunBudgetStatus.New:
                switch (next)
                {
                    case ScheduledPayRunBudgetStatus.Queued:
                    case ScheduledPayRunBudgetStatus.Done:
                        return true;
                }
                return false;
            case ScheduledPayRunBudgetStatus.Queued:
                switch (next)
                {
                    case ScheduledPayRunBudgetStatus.Running:
                    case ScheduledPayRunBudgetStatus.Done:
                        return true;
                }
                return false;
            case ScheduledPayRunBudgetStatus.Running:
                return next == ScheduledPayRunBudgetStatus.Done || next == ScheduledPayRunBudgetStatus.Failed;
            case ScheduledPayRunBudgetStatus.Done:
                switch (next)
                {
                    case ScheduledPayRunBudgetStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledPayRunBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pay run budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetTotal()
    {
        return Math.Round(Rate + (double)Version, 2);
    }

    public ScheduledPayRunBudget Copy()
    {
        return new ScheduledPayRunBudget(
            Code,
            Name,
            Visible,
            Rate,
            Team,
            Quantity,
            Tags,
            BatchSize,
            EndsAt,
            Category,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
