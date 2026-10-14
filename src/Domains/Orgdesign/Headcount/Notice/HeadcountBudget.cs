namespace Acme.Workforce.Orgdesign.Headcount.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount budget is in its life cycle.</summary>
public enum HeadcountBudgetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One headcount budget, with what the planners need to act on it.</summary>
public sealed class HeadcountBudget(string code, string title, List<string> tags, int leaveDays, int sequence, string phone, HeadcountBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public int LeaveDays { get; set; } = leaveDays;
    public int Sequence { get; set; } = sequence;
    public string Phone { get; set; } = phone;
    public HeadcountBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a headcount budget in status new.</summary>
    public static HeadcountBudget NewHeadcountBudget(string code, string title)
    {
        return new HeadcountBudget(code, title, new List<string>(), 0, 0, "", HeadcountBudgetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountBudgetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountBudgetStatus next)
    {
        switch (Status)
        {
            case HeadcountBudgetStatus.New:
                switch (next)
                {
                    case HeadcountBudgetStatus.Running:
                        return true;
                }
                return false;
            case HeadcountBudgetStatus.Queued:
                return next == HeadcountBudgetStatus.Done;
            case HeadcountBudgetStatus.Running:
                switch (next)
                {
                    case HeadcountBudgetStatus.Failed:
                        return true;
                }
                return false;
            case HeadcountBudgetStatus.Done:
                switch (next)
                {
                    case HeadcountBudgetStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double WeightedTotal()
    {
        return Math.Round((double)Sequence * (double)LeaveDays, 2);
    }

    public HeadcountBudget Copy()
    {
        return new HeadcountBudget(Code, Title, Tags, LeaveDays, Sequence, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
