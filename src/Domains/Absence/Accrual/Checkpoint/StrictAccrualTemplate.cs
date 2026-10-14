namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict accrual template is in its life cycle.</summary>
public enum StrictAccrualTemplateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a strict accrual template and its current status.</summary>
public sealed class StrictAccrualTemplate(string code, string name, string managerId, double maximum, string label, string ownerId, double budget, string source, string externalRef, int attempts, StrictAccrualTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public double Maximum { get; set; } = maximum;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public double Budget { get; set; } = budget;
    public string Source { get; set; } = source;
    public string ExternalRef { get; set; } = externalRef;
    public int Attempts { get; set; } = attempts;
    public StrictAccrualTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a strict accrual template in status draft.</summary>
    public static StrictAccrualTemplate NewStrictAccrualTemplate(string code, string name)
    {
        return new StrictAccrualTemplate(
            code,
            name,
            "",
            0.0,
            "",
            "",
            0.0,
            "",
            "",
            0,
            StrictAccrualTemplateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAccrualTemplateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAccrualTemplateStatus next)
    {
        switch (Status)
        {
            case StrictAccrualTemplateStatus.Draft:
                return next == StrictAccrualTemplateStatus.Suspended;
            case StrictAccrualTemplateStatus.Pending:
                switch (next)
                {
                    case StrictAccrualTemplateStatus.Suspended:
                        return true;
                }
                return false;
            case StrictAccrualTemplateStatus.Active:
                switch (next)
                {
                    case StrictAccrualTemplateStatus.Suspended:
                    case StrictAccrualTemplateStatus.Closed:
                        return true;
                }
                return false;
            case StrictAccrualTemplateStatus.Suspended:
                switch (next)
                {
                    case StrictAccrualTemplateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictAccrualTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict accrual template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"strict accrual template"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Budget * (double)Attempts, 2);
    }

    public StrictAccrualTemplate Copy()
    {
        return new StrictAccrualTemplate(
            Code,
            Name,
            ManagerId,
            Maximum,
            Label,
            OwnerId,
            Budget,
            Source,
            ExternalRef,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
