namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception template is in its life cycle.</summary>
public enum ExceptionTemplateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a exception template and its current status.</summary>
public sealed class ExceptionTemplate(string code, string title, string currency, bool archived, string description, double maximum, string managerId, int windowDays, string reviewer, double cost, ExceptionTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public bool Archived { get; set; } = archived;
    public string Description { get; set; } = description;
    public double Maximum { get; set; } = maximum;
    public string ManagerId { get; set; } = managerId;
    public int WindowDays { get; set; } = windowDays;
    public string Reviewer { get; set; } = reviewer;
    public double Cost { get; set; } = cost;
    public ExceptionTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a exception template in status new.</summary>
    public static ExceptionTemplate NewExceptionTemplate(string code, string title)
    {
        return new ExceptionTemplate(code, title, "", false, "", 0.0, "", 0, "", 0.0, ExceptionTemplateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionTemplateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionTemplateStatus next)
    {
        switch (Status)
        {
            case ExceptionTemplateStatus.New:
                switch (next)
                {
                    case ExceptionTemplateStatus.Done:
                        return true;
                }
                return false;
            case ExceptionTemplateStatus.Queued:
                return next == ExceptionTemplateStatus.Failed;
            case ExceptionTemplateStatus.Running:
                return next == ExceptionTemplateStatus.Done;
            case ExceptionTemplateStatus.Done:
                return next == ExceptionTemplateStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ExceptionTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Cost - (double)WindowDays, 2);
    }

    public ExceptionTemplate Copy()
    {
        return new ExceptionTemplate(
            Code,
            Title,
            Currency,
            Archived,
            Description,
            Maximum,
            ManagerId,
            WindowDays,
            Reviewer,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
