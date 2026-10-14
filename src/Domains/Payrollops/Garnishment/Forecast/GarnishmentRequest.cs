namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment request is in its life cycle.</summary>
public enum GarnishmentRequestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a garnishment request and its current status.</summary>
public sealed class GarnishmentRequest(string code, string title, string contactEmail, double maximum, double fte, double floorValue, string employeeId, string salaryBand, int priority, GarnishmentRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public double Maximum { get; set; } = maximum;
    public double Fte { get; set; } = fte;
    public double FloorValue { get; set; } = floorValue;
    public string EmployeeId { get; set; } = employeeId;
    public string SalaryBand { get; set; } = salaryBand;
    public int Priority { get; set; } = priority;
    public GarnishmentRequestStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment request in status idle.</summary>
    public static GarnishmentRequest NewGarnishmentRequest(string code, string title)
    {
        return new GarnishmentRequest(code, title, "", 0.0, 0.0, 0.0, "", "", 0, GarnishmentRequestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentRequestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentRequestStatus next)
    {
        switch (Status)
        {
            case GarnishmentRequestStatus.Idle:
                switch (next)
                {
                    case GarnishmentRequestStatus.Draining:
                    case GarnishmentRequestStatus.Stopped:
                        return true;
                }
                return false;
            case GarnishmentRequestStatus.Warming:
                switch (next)
                {
                    case GarnishmentRequestStatus.Ready:
                    case GarnishmentRequestStatus.Stopped:
                        return true;
                }
                return false;
            case GarnishmentRequestStatus.Ready:
                switch (next)
                {
                    case GarnishmentRequestStatus.Stopped:
                        return true;
                }
                return false;
            case GarnishmentRequestStatus.Draining:
                switch (next)
                {
                    case GarnishmentRequestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"garnishment request"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Maximum - Fte, 2);
    }

    public GarnishmentRequest Copy()
    {
        return new GarnishmentRequest(
            Code,
            Title,
            ContactEmail,
            Maximum,
            Fte,
            FloorValue,
            EmployeeId,
            SalaryBand,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
