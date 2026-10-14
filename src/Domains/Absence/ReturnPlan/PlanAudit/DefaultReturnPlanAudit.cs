namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default return plan audit is in its life cycle.</summary>
public enum DefaultReturnPlanAuditStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One default return plan audit, with what the planners need to act on it.</summary>
public sealed class DefaultReturnPlanAudit(string code, string title, Dictionary<string, string> attributes, string category, string phone, string currency, int windowDays, string costCenter, DefaultReturnPlanAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Category { get; set; } = category;
    public string Phone { get; set; } = phone;
    public string Currency { get; set; } = currency;
    public int WindowDays { get; set; } = windowDays;
    public string CostCenter { get; set; } = costCenter;
    public DefaultReturnPlanAuditStatus Status { get; set; } = status;

    /// <summary>Starts a default return plan audit in status requested.</summary>
    public static DefaultReturnPlanAudit NewDefaultReturnPlanAudit(string code, string title)
    {
        return new DefaultReturnPlanAudit(
            code,
            title,
            new Dictionary<string, string>(),
            "",
            "",
            "",
            0,
            "",
            DefaultReturnPlanAuditStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultReturnPlanAuditStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultReturnPlanAuditStatus next)
    {
        switch (Status)
        {
            case DefaultReturnPlanAuditStatus.Requested:
                return next == DefaultReturnPlanAuditStatus.Granted;
            case DefaultReturnPlanAuditStatus.Granted:
                switch (next)
                {
                    case DefaultReturnPlanAuditStatus.Expired:
                        return true;
                }
                return false;
            case DefaultReturnPlanAuditStatus.Revoked:
                switch (next)
                {
                    case DefaultReturnPlanAuditStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultReturnPlanAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default return plan audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default return plan audit"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public DefaultReturnPlanAudit Copy()
    {
        return new DefaultReturnPlanAudit(
            Code,
            Title,
            Attributes,
            Category,
            Phone,
            Currency,
            WindowDays,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
