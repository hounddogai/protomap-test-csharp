namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip summary is in its life cycle.</summary>
public enum PayslipSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A payslip summary. Its code is unique within a tenant.</summary>
public sealed class PayslipSummary(string code, string name, string title, string channel, int attempts, string ownerId, double rate, bool archived, string department, PayslipSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public int Attempts { get; set; } = attempts;
    public string OwnerId { get; set; } = ownerId;
    public double Rate { get; set; } = rate;
    public bool Archived { get; set; } = archived;
    public string Department { get; set; } = department;
    public PayslipSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a payslip summary in status requested.</summary>
    public static PayslipSummary NewPayslipSummary(string code, string name)
    {
        return new PayslipSummary(code, name, "", "", 0, "", 0.0, false, "", PayslipSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipSummaryStatus next)
    {
        switch (Status)
        {
            case PayslipSummaryStatus.Requested:
                switch (next)
                {
                    case PayslipSummaryStatus.Revoked:
                        return true;
                }
                return false;
            case PayslipSummaryStatus.Granted:
                switch (next)
                {
                    case PayslipSummaryStatus.Revoked:
                    case PayslipSummaryStatus.Expired:
                        return true;
                }
                return false;
            case PayslipSummaryStatus.Revoked:
                return next == PayslipSummaryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PayslipSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Attempts * Rate, 2);
    }

    public PayslipSummary Copy()
    {
        return new PayslipSummary(Code, Name, Title, Channel, Attempts, OwnerId, Rate, Archived, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
