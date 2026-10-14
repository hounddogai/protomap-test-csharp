namespace Acme.Workforce.Absence.ReturnPlan.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan item is in its life cycle.</summary>
public enum ReturnPlanItemStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A return plan item as the return plan team keeps it.</summary>
public sealed class ReturnPlanItem(string code, string title, int retries, string contactEmail, int version, string skill, string managerId, int windowDays, ReturnPlanItemStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string ContactEmail { get; set; } = contactEmail;
    public int Version { get; set; } = version;
    public string Skill { get; set; } = skill;
    public string ManagerId { get; set; } = managerId;
    public int WindowDays { get; set; } = windowDays;
    public ReturnPlanItemStatus Status { get; set; } = status;

    /// <summary>Starts a return plan item in status created.</summary>
    public static ReturnPlanItem NewReturnPlanItem(string code, string title)
    {
        return new ReturnPlanItem(code, title, 0, "", 0, "", "", 0, ReturnPlanItemStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanItemStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanItemStatus next)
    {
        switch (Status)
        {
            case ReturnPlanItemStatus.Created:
                switch (next)
                {
                    case ReturnPlanItemStatus.Published:
                    case ReturnPlanItemStatus.Retired:
                        return true;
                }
                return false;
            case ReturnPlanItemStatus.Validated:
                switch (next)
                {
                    case ReturnPlanItemStatus.Published:
                    case ReturnPlanItemStatus.Retired:
                        return true;
                }
                return false;
            case ReturnPlanItemStatus.Published:
                return next == ReturnPlanItemStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ReturnPlanItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetVolume()
    {
        return Math.Round((double)Version * (double)WindowDays, 2);
    }

    public ReturnPlanItem Copy()
    {
        return new ReturnPlanItem(Code, Title, Retries, ContactEmail, Version, Skill, ManagerId, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
