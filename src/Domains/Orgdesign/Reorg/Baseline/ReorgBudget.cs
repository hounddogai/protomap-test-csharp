namespace Acme.Workforce.Orgdesign.Reorg.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg budget is in its life cycle.</summary>
public enum ReorgBudgetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A reorg budget. Its code is unique within a tenant.</summary>
public sealed class ReorgBudget(string code, string title, string timezone, int leaveDays, int createdAt, string description, ReorgBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public int LeaveDays { get; set; } = leaveDays;
    public int CreatedAt { get; set; } = createdAt;
    public string Description { get; set; } = description;
    public ReorgBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a reorg budget in status new.</summary>
    public static ReorgBudget NewReorgBudget(string code, string title)
    {
        return new ReorgBudget(code, title, "", 0, 0, "", ReorgBudgetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgBudgetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgBudgetStatus next)
    {
        switch (Status)
        {
            case ReorgBudgetStatus.New:
                return next == ReorgBudgetStatus.Queued || next == ReorgBudgetStatus.Running;
            case ReorgBudgetStatus.Queued:
                switch (next)
                {
                    case ReorgBudgetStatus.Failed:
                        return true;
                }
                return false;
            case ReorgBudgetStatus.Running:
                return next == ReorgBudgetStatus.Done;
            case ReorgBudgetStatus.Done:
                switch (next)
                {
                    case ReorgBudgetStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)LeaveDays * (double)CreatedAt, 2);
    }

    public ReorgBudget Copy()
    {
        return new ReorgBudget(Code, Title, Timezone, LeaveDays, CreatedAt, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
