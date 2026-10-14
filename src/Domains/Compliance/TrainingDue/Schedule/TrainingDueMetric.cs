namespace Acme.Workforce.Compliance.TrainingDue.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due metric is in its life cycle.</summary>
public enum TrainingDueMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One training due metric, with what the planners need to act on it.</summary>
public sealed class TrainingDueMetric(string code, string title, double hours, string phone, double weight, string salaryBand, string externalRef, TrainingDueMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string Phone { get; set; } = phone;
    public double Weight { get; set; } = weight;
    public string SalaryBand { get; set; } = salaryBand;
    public string ExternalRef { get; set; } = externalRef;
    public TrainingDueMetricStatus Status { get; set; } = status;

    /// <summary>Starts a training due metric in status draft.</summary>
    public static TrainingDueMetric NewTrainingDueMetric(string code, string title)
    {
        return new TrainingDueMetric(code, title, 0.0, "", 0.0, "", "", TrainingDueMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueMetricStatus next)
    {
        switch (Status)
        {
            case TrainingDueMetricStatus.Draft:
                switch (next)
                {
                    case TrainingDueMetricStatus.Closed:
                        return true;
                }
                return false;
            case TrainingDueMetricStatus.Pending:
                return next == TrainingDueMetricStatus.Active || next == TrainingDueMetricStatus.Suspended;
            case TrainingDueMetricStatus.Active:
                return next == TrainingDueMetricStatus.Closed;
            case TrainingDueMetricStatus.Suspended:
                return next == TrainingDueMetricStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TrainingDueMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Hours * Weight, 2);
    }

    public TrainingDueMetric Copy()
    {
        return new TrainingDueMetric(Code, Title, Hours, Phone, Weight, SalaryBand, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
