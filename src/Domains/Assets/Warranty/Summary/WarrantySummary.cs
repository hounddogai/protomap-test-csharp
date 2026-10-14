namespace Acme.Workforce.Assets.Warranty.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty summary is in its life cycle.</summary>
public enum WarrantySummaryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a warranty summary and its current status.</summary>
public sealed class WarrantySummary(string code, string label, string jobCode, string currency, string costCenter, int endsAt, bool active, double minimum, double maximum, string employeeId, string skill, WarrantySummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public string Currency { get; set; } = currency;
    public string CostCenter { get; set; } = costCenter;
    public int EndsAt { get; set; } = endsAt;
    public bool Active { get; set; } = active;
    public double Minimum { get; set; } = minimum;
    public double Maximum { get; set; } = maximum;
    public string EmployeeId { get; set; } = employeeId;
    public string Skill { get; set; } = skill;
    public WarrantySummaryStatus Status { get; set; } = status;

    /// <summary>Starts a warranty summary in status idle.</summary>
    public static WarrantySummary NewWarrantySummary(string code, string label)
    {
        return new WarrantySummary(code, label, "", "", "", 0, false, 0.0, 0.0, "", "", WarrantySummaryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantySummaryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantySummaryStatus next)
    {
        switch (Status)
        {
            case WarrantySummaryStatus.Idle:
                return next == WarrantySummaryStatus.Warming || next == WarrantySummaryStatus.Draining;
            case WarrantySummaryStatus.Warming:
                return next == WarrantySummaryStatus.Ready || next == WarrantySummaryStatus.Stopped;
            case WarrantySummaryStatus.Ready:
                return next == WarrantySummaryStatus.Draining;
            case WarrantySummaryStatus.Draining:
                return next == WarrantySummaryStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(WarrantySummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveValue()
    {
        return Math.Round(Maximum + Minimum, 2);
    }

    public WarrantySummary Copy()
    {
        return new WarrantySummary(
            Code,
            Label,
            JobCode,
            Currency,
            CostCenter,
            EndsAt,
            Active,
            Minimum,
            Maximum,
            EmployeeId,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
