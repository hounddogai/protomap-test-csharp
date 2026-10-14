namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor template is in its life cycle.</summary>
public enum VisitorTemplateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a visitor template and its current status.</summary>
public sealed class VisitorTemplate(string code, string name, double ratio, string department, bool visible, string jobCode, VisitorTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public string Department { get; set; } = department;
    public bool Visible { get; set; } = visible;
    public string JobCode { get; set; } = jobCode;
    public VisitorTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a visitor template in status requested.</summary>
    public static VisitorTemplate NewVisitorTemplate(string code, string name)
    {
        return new VisitorTemplate(code, name, 0.0, "", false, "", VisitorTemplateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorTemplateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorTemplateStatus next)
    {
        switch (Status)
        {
            case VisitorTemplateStatus.Requested:
                switch (next)
                {
                    case VisitorTemplateStatus.Revoked:
                        return true;
                }
                return false;
            case VisitorTemplateStatus.Granted:
                return next == VisitorTemplateStatus.Expired;
            case VisitorTemplateStatus.Revoked:
                switch (next)
                {
                    case VisitorTemplateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public VisitorTemplate Copy()
    {
        return new VisitorTemplate(Code, Name, Ratio, Department, Visible, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
