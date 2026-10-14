namespace Acme.Workforce.Payrollops.PayRun.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run exception is in its life cycle.</summary>
public enum PayRunExceptionStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One pay run exception, with what the planners need to act on it.</summary>
public sealed class PayRunException(string code, string label, string notes, string jobCode, string category, string managerId, string channel, string title, string costCenter, int durationMinutes, double minimum, PayRunExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string JobCode { get; set; } = jobCode;
    public string Category { get; set; } = category;
    public string ManagerId { get; set; } = managerId;
    public string Channel { get; set; } = channel;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Minimum { get; set; } = minimum;
    public PayRunExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a pay run exception in status created.</summary>
    public static PayRunException NewPayRunException(string code, string label)
    {
        return new PayRunException(code, label, "", "", "", "", "", "", "", 0, 0.0, PayRunExceptionStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunExceptionStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunExceptionStatus next)
    {
        switch (Status)
        {
            case PayRunExceptionStatus.Created:
                return next == PayRunExceptionStatus.Validated || next == PayRunExceptionStatus.Retired;
            case PayRunExceptionStatus.Validated:
                return next == PayRunExceptionStatus.Published;
            case PayRunExceptionStatus.Published:
                switch (next)
                {
                    case PayRunExceptionStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pay run exception"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Minimum - (double)DurationMinutes, 2);
    }

    public PayRunException Copy()
    {
        return new PayRunException(
            Code,
            Label,
            Notes,
            JobCode,
            Category,
            ManagerId,
            Channel,
            Title,
            CostCenter,
            DurationMinutes,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
