namespace Acme.Workforce.Compliance.TrainingDue.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due summary is in its life cycle.</summary>
public enum TrainingDueSummaryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One training due summary, with what the planners need to act on it.</summary>
public sealed class TrainingDueSummary(string code, string name, int sequence, string jobCode, string currency, bool approved, int updatedAt, string salaryBand, string costCenter, double fte, TrainingDueSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public string JobCode { get; set; } = jobCode;
    public string Currency { get; set; } = currency;
    public bool Approved { get; set; } = approved;
    public int UpdatedAt { get; set; } = updatedAt;
    public string SalaryBand { get; set; } = salaryBand;
    public string CostCenter { get; set; } = costCenter;
    public double Fte { get; set; } = fte;
    public TrainingDueSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a training due summary in status idle.</summary>
    public static TrainingDueSummary NewTrainingDueSummary(string code, string name)
    {
        return new TrainingDueSummary(code, name, 0, "", "", false, 0, "", "", 0.0, TrainingDueSummaryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueSummaryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueSummaryStatus next)
    {
        switch (Status)
        {
            case TrainingDueSummaryStatus.Idle:
                switch (next)
                {
                    case TrainingDueSummaryStatus.Ready:
                        return true;
                }
                return false;
            case TrainingDueSummaryStatus.Warming:
                return next == TrainingDueSummaryStatus.Draining;
            case TrainingDueSummaryStatus.Ready:
                switch (next)
                {
                    case TrainingDueSummaryStatus.Draining:
                    case TrainingDueSummaryStatus.Stopped:
                        return true;
                }
                return false;
            case TrainingDueSummaryStatus.Draining:
                return next == TrainingDueSummaryStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TrainingDueSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Sequence - (double)UpdatedAt, 2);
    }

    public TrainingDueSummary Copy()
    {
        return new TrainingDueSummary(
            Code,
            Name,
            Sequence,
            JobCode,
            Currency,
            Approved,
            UpdatedAt,
            SalaryBand,
            CostCenter,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
