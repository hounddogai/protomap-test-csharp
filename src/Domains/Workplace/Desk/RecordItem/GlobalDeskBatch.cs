namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global desk batch is in its life cycle.</summary>
public enum GlobalDeskBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One global desk batch, with what the planners need to act on it.</summary>
public sealed class GlobalDeskBatch(string code, string label, string title, string skill, string phone, string channel, string currency, double budget, GlobalDeskBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public string Phone { get; set; } = phone;
    public string Channel { get; set; } = channel;
    public string Currency { get; set; } = currency;
    public double Budget { get; set; } = budget;
    public GlobalDeskBatchStatus Status { get; set; } = status;

    /// <summary>Starts a global desk batch in status new.</summary>
    public static GlobalDeskBatch NewGlobalDeskBatch(string code, string label)
    {
        return new GlobalDeskBatch(code, label, "", "", "", "", "", 0.0, GlobalDeskBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalDeskBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalDeskBatchStatus next)
    {
        switch (Status)
        {
            case GlobalDeskBatchStatus.New:
                switch (next)
                {
                    case GlobalDeskBatchStatus.Queued:
                        return true;
                }
                return false;
            case GlobalDeskBatchStatus.Queued:
                switch (next)
                {
                    case GlobalDeskBatchStatus.Running:
                    case GlobalDeskBatchStatus.Failed:
                        return true;
                }
                return false;
            case GlobalDeskBatchStatus.Running:
                switch (next)
                {
                    case GlobalDeskBatchStatus.Failed:
                        return true;
                }
                return false;
            case GlobalDeskBatchStatus.Done:
                return next == GlobalDeskBatchStatus.Failed;
        }
        return false;
    }

    public void MoveTo(GlobalDeskBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global desk batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public GlobalDeskBatch Copy()
    {
        return new GlobalDeskBatch(Code, Label, Title, Skill, Phone, Channel, Currency, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
