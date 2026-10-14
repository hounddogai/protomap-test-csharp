namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work template is in its life cycle.</summary>
public enum StatementOfWorkTemplateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a statement of work template and its current status.</summary>
public sealed class StatementOfWorkTemplate(string code, string label, string badge, bool active, string department, string externalRef, double score, StatementOfWorkTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public bool Active { get; set; } = active;
    public string Department { get; set; } = department;
    public string ExternalRef { get; set; } = externalRef;
    public double Score { get; set; } = score;
    public StatementOfWorkTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work template in status draft.</summary>
    public static StatementOfWorkTemplate NewStatementOfWorkTemplate(string code, string label)
    {
        return new StatementOfWorkTemplate(code, label, "", false, "", "", 0.0, StatementOfWorkTemplateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkTemplateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkTemplateStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkTemplateStatus.Draft:
                switch (next)
                {
                    case StatementOfWorkTemplateStatus.Closed:
                        return true;
                }
                return false;
            case StatementOfWorkTemplateStatus.Pending:
                return next == StatementOfWorkTemplateStatus.Suspended;
            case StatementOfWorkTemplateStatus.Active:
                return next == StatementOfWorkTemplateStatus.Suspended || next == StatementOfWorkTemplateStatus.Closed;
            case StatementOfWorkTemplateStatus.Suspended:
                switch (next)
                {
                    case StatementOfWorkTemplateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public StatementOfWorkTemplate Copy()
    {
        return new StatementOfWorkTemplate(Code, Label, Badge, Active, Department, ExternalRef, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
