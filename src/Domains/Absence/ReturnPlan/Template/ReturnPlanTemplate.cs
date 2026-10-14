namespace Acme.Workforce.Absence.ReturnPlan.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan template is in its life cycle.</summary>
public enum ReturnPlanTemplateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One return plan template, with what the planners need to act on it.</summary>
public sealed class ReturnPlanTemplate(string code, string label, string checksum, int attempts, string category, bool approved, ReturnPlanTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public int Attempts { get; set; } = attempts;
    public string Category { get; set; } = category;
    public bool Approved { get; set; } = approved;
    public ReturnPlanTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a return plan template in status created.</summary>
    public static ReturnPlanTemplate NewReturnPlanTemplate(string code, string label)
    {
        return new ReturnPlanTemplate(code, label, "", 0, "", false, ReturnPlanTemplateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanTemplateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanTemplateStatus next)
    {
        switch (Status)
        {
            case ReturnPlanTemplateStatus.Created:
                switch (next)
                {
                    case ReturnPlanTemplateStatus.Published:
                        return true;
                }
                return false;
            case ReturnPlanTemplateStatus.Validated:
                return next == ReturnPlanTemplateStatus.Published || next == ReturnPlanTemplateStatus.Retired;
            case ReturnPlanTemplateStatus.Published:
                return next == ReturnPlanTemplateStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ReturnPlanTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"return plan template"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public ReturnPlanTemplate Copy()
    {
        return new ReturnPlanTemplate(Code, Label, Checksum, Attempts, Category, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
