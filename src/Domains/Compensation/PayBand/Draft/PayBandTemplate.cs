namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band template is in its life cycle.</summary>
public enum PayBandTemplateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One pay band template, with what the planners need to act on it.</summary>
public sealed class PayBandTemplate(string code, string name, string location, double hours, int quantity, PayBandTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public double Hours { get; set; } = hours;
    public int Quantity { get; set; } = quantity;
    public PayBandTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a pay band template in status idle.</summary>
    public static PayBandTemplate NewPayBandTemplate(string code, string name)
    {
        return new PayBandTemplate(code, name, "", 0.0, 0, PayBandTemplateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandTemplateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandTemplateStatus next)
    {
        switch (Status)
        {
            case PayBandTemplateStatus.Idle:
                switch (next)
                {
                    case PayBandTemplateStatus.Warming:
                    case PayBandTemplateStatus.Ready:
                        return true;
                }
                return false;
            case PayBandTemplateStatus.Warming:
                switch (next)
                {
                    case PayBandTemplateStatus.Stopped:
                        return true;
                }
                return false;
            case PayBandTemplateStatus.Ready:
                switch (next)
                {
                    case PayBandTemplateStatus.Draining:
                    case PayBandTemplateStatus.Stopped:
                        return true;
                }
                return false;
            case PayBandTemplateStatus.Draining:
                switch (next)
                {
                    case PayBandTemplateStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pay band template"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Quantity * Hours, 2);
    }

    public PayBandTemplate Copy()
    {
        return new PayBandTemplate(Code, Name, Location, Hours, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
