namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor plan is in its life cycle.</summary>
public enum VisitorPlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A visitor plan. Its code is unique within a tenant.</summary>
public sealed class VisitorPlan(string code, string label, string source, string locale, string contactEmail, Dictionary<string, string> attributes, int capacity, double budget, VisitorPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public string Locale { get; set; } = locale;
    public string ContactEmail { get; set; } = contactEmail;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Capacity { get; set; } = capacity;
    public double Budget { get; set; } = budget;
    public VisitorPlanStatus Status { get; set; } = status;

    /// <summary>Starts a visitor plan in status draft.</summary>
    public static VisitorPlan NewVisitorPlan(string code, string label)
    {
        return new VisitorPlan(
            code,
            label,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            0,
            0.0,
            VisitorPlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorPlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorPlanStatus next)
    {
        switch (Status)
        {
            case VisitorPlanStatus.Draft:
                return next == VisitorPlanStatus.Closed;
            case VisitorPlanStatus.Pending:
                return next == VisitorPlanStatus.Active || next == VisitorPlanStatus.Suspended;
            case VisitorPlanStatus.Active:
                switch (next)
                {
                    case VisitorPlanStatus.Suspended:
                    case VisitorPlanStatus.Closed:
                        return true;
                }
                return false;
            case VisitorPlanStatus.Suspended:
                switch (next)
                {
                    case VisitorPlanStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double ProjectedLoad()
    {
        return Math.Round(Budget + (double)Capacity, 2);
    }

    public VisitorPlan Copy()
    {
        return new VisitorPlan(Code, Label, Source, Locale, ContactEmail, Attributes, Capacity, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
