namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual bonus pool template is in its life cycle.</summary>
public enum ManualBonusPoolTemplateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One manual bonus pool template, with what the planners need to act on it.</summary>
public sealed class ManualBonusPoolTemplate(string code, string title, int version, int limit, string name, ManualBonusPoolTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public int Limit { get; set; } = limit;
    public string Name { get; set; } = name;
    public ManualBonusPoolTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a manual bonus pool template in status planned.</summary>
    public static ManualBonusPoolTemplate NewManualBonusPoolTemplate(string code, string title)
    {
        return new ManualBonusPoolTemplate(code, title, 0, 0, "", ManualBonusPoolTemplateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBonusPoolTemplateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBonusPoolTemplateStatus next)
    {
        switch (Status)
        {
            case ManualBonusPoolTemplateStatus.Planned:
                switch (next)
                {
                    case ManualBonusPoolTemplateStatus.InProgress:
                        return true;
                }
                return false;
            case ManualBonusPoolTemplateStatus.Scheduled:
                return next == ManualBonusPoolTemplateStatus.InProgress;
            case ManualBonusPoolTemplateStatus.InProgress:
                return next == ManualBonusPoolTemplateStatus.Completed || next == ManualBonusPoolTemplateStatus.Cancelled;
            case ManualBonusPoolTemplateStatus.Completed:
                switch (next)
                {
                    case ManualBonusPoolTemplateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualBonusPoolTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual bonus pool template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Limit * (double)Version, 2);
    }

    public ManualBonusPoolTemplate Copy()
    {
        return new ManualBonusPoolTemplate(Code, Title, Version, Limit, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
