namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global garnishment target is in its life cycle.</summary>
public enum GlobalGarnishmentTargetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A global garnishment target as the garnishment team keeps it.</summary>
public sealed class GlobalGarnishmentTarget(string code, string title, string country, double margin, double hours, string grade, bool active, string unit, GlobalGarnishmentTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public double Margin { get; set; } = margin;
    public double Hours { get; set; } = hours;
    public string Grade { get; set; } = grade;
    public bool Active { get; set; } = active;
    public string Unit { get; set; } = unit;
    public GlobalGarnishmentTargetStatus Status { get; set; } = status;

    /// <summary>Starts a global garnishment target in status new.</summary>
    public static GlobalGarnishmentTarget NewGlobalGarnishmentTarget(string code, string title)
    {
        return new GlobalGarnishmentTarget(
            code,
            title,
            "",
            0.0,
            0.0,
            "",
            false,
            "",
            GlobalGarnishmentTargetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalGarnishmentTargetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalGarnishmentTargetStatus next)
    {
        switch (Status)
        {
            case GlobalGarnishmentTargetStatus.New:
                switch (next)
                {
                    case GlobalGarnishmentTargetStatus.Running:
                    case GlobalGarnishmentTargetStatus.Done:
                        return true;
                }
                return false;
            case GlobalGarnishmentTargetStatus.Queued:
                return next == GlobalGarnishmentTargetStatus.Running || next == GlobalGarnishmentTargetStatus.Failed;
            case GlobalGarnishmentTargetStatus.Running:
                switch (next)
                {
                    case GlobalGarnishmentTargetStatus.Done:
                    case GlobalGarnishmentTargetStatus.Failed:
                        return true;
                }
                return false;
            case GlobalGarnishmentTargetStatus.Done:
                return next == GlobalGarnishmentTargetStatus.Failed;
        }
        return false;
    }

    public void MoveTo(GlobalGarnishmentTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global garnishment target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetVolume()
    {
        return Math.Round(Hours * Margin, 2);
    }

    public GlobalGarnishmentTarget Copy()
    {
        return new GlobalGarnishmentTarget(Code, Title, Country, Margin, Hours, Grade, Active, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
