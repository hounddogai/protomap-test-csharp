namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip exception is in its life cycle.</summary>
public enum PayslipExceptionStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A payslip exception as the payslip team keeps it.</summary>
public sealed class PayslipException(string code, string title, bool approved, string contactEmail, Dictionary<string, string> attributes, int attempts, double cost, string channel, double amount, List<string> tags, string name, PayslipExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public string ContactEmail { get; set; } = contactEmail;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Attempts { get; set; } = attempts;
    public double Cost { get; set; } = cost;
    public string Channel { get; set; } = channel;
    public double Amount { get; set; } = amount;
    public List<string> Tags { get; set; } = tags;
    public string Name { get; set; } = name;
    public PayslipExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a payslip exception in status open.</summary>
    public static PayslipException NewPayslipException(string code, string title)
    {
        return new PayslipException(
            code,
            title,
            false,
            "",
            new Dictionary<string, string>(),
            0,
            0.0,
            "",
            0.0,
            new List<string>(),
            "",
            PayslipExceptionStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipExceptionStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipExceptionStatus next)
    {
        switch (Status)
        {
            case PayslipExceptionStatus.Open:
                return next == PayslipExceptionStatus.Resolved || next == PayslipExceptionStatus.Archived;
            case PayslipExceptionStatus.InReview:
                return next == PayslipExceptionStatus.Archived;
            case PayslipExceptionStatus.Resolved:
                return next == PayslipExceptionStatus.Archived;
            case PayslipExceptionStatus.Reopened:
                return next == PayslipExceptionStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PayslipExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"payslip exception"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
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

    public double NetTotal()
    {
        return Math.Round(Amount * (double)Attempts, 2);
    }

    public PayslipException Copy()
    {
        return new PayslipException(
            Code,
            Title,
            Approved,
            ContactEmail,
            Attributes,
            Attempts,
            Cost,
            Channel,
            Amount,
            Tags,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
