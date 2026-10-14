namespace Acme.Workforce.Contractors.StatementOfWork.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict statement of work metric is in its life cycle.</summary>
public enum StrictStatementOfWorkMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a strict statement of work metric and its current status.</summary>
public sealed class StrictStatementOfWorkMetric(string code, string name, string externalRef, double threshold, string title, string label, string region, int attempts, int updatedAt, StrictStatementOfWorkMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public double Threshold { get; set; } = threshold;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public int Attempts { get; set; } = attempts;
    public int UpdatedAt { get; set; } = updatedAt;
    public StrictStatementOfWorkMetricStatus Status { get; set; } = status;

    /// <summary>Starts a strict statement of work metric in status proposed.</summary>
    public static StrictStatementOfWorkMetric NewStrictStatementOfWorkMetric(string code, string name)
    {
        return new StrictStatementOfWorkMetric(
            code,
            name,
            "",
            0.0,
            "",
            "",
            "",
            0,
            0,
            StrictStatementOfWorkMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictStatementOfWorkMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictStatementOfWorkMetricStatus next)
    {
        switch (Status)
        {
            case StrictStatementOfWorkMetricStatus.Proposed:
                switch (next)
                {
                    case StrictStatementOfWorkMetricStatus.Rejected:
                    case StrictStatementOfWorkMetricStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictStatementOfWorkMetricStatus.Approved:
                return next == StrictStatementOfWorkMetricStatus.Rejected || next == StrictStatementOfWorkMetricStatus.Withdrawn;
            case StrictStatementOfWorkMetricStatus.Rejected:
                switch (next)
                {
                    case StrictStatementOfWorkMetricStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictStatementOfWorkMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict statement of work metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Threshold * (double)Attempts, 2);
    }

    public StrictStatementOfWorkMetric Copy()
    {
        return new StrictStatementOfWorkMetric(
            Code,
            Name,
            ExternalRef,
            Threshold,
            Title,
            Label,
            Region,
            Attempts,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
